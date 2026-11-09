using System.Security.Claims;
using System.Text.Json;
using StackExchange.Redis;

namespace ApiGateway.Middleware;

public class TokenRevocationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IConnectionMultiplexer? _redis;
    private readonly ILogger<TokenRevocationMiddleware> _logger;

    public const string JtiKeyPrefix = "auth:revoked:jti:";
    public const string UserCutoffKeyPrefix = "auth:revoked-before:";

    public TokenRevocationMiddleware(
        RequestDelegate next,
        IServiceProvider serviceProvider,
        ILogger<TokenRevocationMiddleware> logger)
    {
        _next = next;
        _redis = serviceProvider.GetService<IConnectionMultiplexer>();
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // 1. Skip unauthenticated requests
        if (context.User.Identity?.IsAuthenticated != true)
        {
            await _next(context);
            return;
        }

        // 2. Extract token claims
        var jti = context.User.FindFirstValue("jti")
                  ?? context.User.FindFirstValue("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");

        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier)
                     ?? context.User.FindFirstValue("sub")
                     ?? context.User.FindFirstValue("id");

        var iatClaim = context.User.FindFirstValue("iat");
        long? issuedAtUnix = long.TryParse(iatClaim, out var iat) ? iat : null;

        // If Redis is unreachable, fail-open gracefully while logging a security warning
        if (_redis == null || !_redis.IsConnected)
        {
            _logger.LogWarning("Redis is not connected. Skipping token revocation check for User {UserId}.", userId);
            await _next(context);
            return;
        }

        try
        {
            var db = _redis.GetDatabase();
            var hasJti = !string.IsNullOrWhiteSpace(jti);
            var hasUser = !string.IsNullOrWhiteSpace(userId);

            if (hasJti && hasUser)
            {
                // Single-roundtrip atomic MGET to check both JTI denylist and user cutoff
                RedisKey[] keys =
                {
                    $"{JtiKeyPrefix}{jti}",
                    $"{UserCutoffKeyPrefix}{userId}"
                };

                var values = await db.StringGetAsync(keys);

                var isJtiRevoked = values[0].HasValue;
                var isCutoffRevoked = values[1].HasValue &&
                                      long.TryParse(values[1], out var cutoffUnix) &&
                                      (!issuedAtUnix.HasValue || issuedAtUnix.Value <= cutoffUnix);

                if (isJtiRevoked || isCutoffRevoked)
                {
                    _logger.LogWarning(
                        "Blocked revoked token at API Gateway for User {UserId}, Jti {Jti}. Reason: {Reason}",
                        userId, jti, isJtiRevoked ? "JTI_REVOKED" : "USER_CUTOFF_EXCEEDED");

                    await WriteRevokedResponseAsync(context);
                    return;
                }
            }
            else if (hasJti)
            {
                var isJtiRevoked = await db.KeyExistsAsync($"{JtiKeyPrefix}{jti}");
                if (isJtiRevoked)
                {
                    _logger.LogWarning("Blocked revoked token Jti {Jti} at API Gateway.", jti);
                    await WriteRevokedResponseAsync(context);
                    return;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while verifying token revocation in Redis. Failing open gracefully.");
        }

        await _next(context);
    }

    private static async Task WriteRevokedResponseAsync(HttpContext context)
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        context.Response.ContentType = "application/json";

        var errorResponse = new
        {
            isSuccess = false,
            message = "Token has been revoked.",
            statusCode = 401,
            errorCode = "AUTH_TOKEN_REVOKED"
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(errorResponse));
    }
}

