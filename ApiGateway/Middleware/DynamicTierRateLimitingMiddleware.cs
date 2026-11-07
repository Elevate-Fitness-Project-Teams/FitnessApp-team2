using System.Security.Claims;
using System.Text.Json;
using StackExchange.Redis;

namespace ApiGateway.Middleware;

public class DynamicTierRateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IConnectionMultiplexer? _redis;
    private readonly ILogger<DynamicTierRateLimitingMiddleware> _logger;

    public DynamicTierRateLimitingMiddleware(
        RequestDelegate next,
        IServiceProvider serviceProvider,
        ILogger<DynamicTierRateLimitingMiddleware> logger)
    {
        _next = next;
        _redis = serviceProvider.GetService<IConnectionMultiplexer>();
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Skip anonymous requests or webhook endpoints
        if (context.User.Identity?.IsAuthenticated != true ||
            context.Request.Path.StartsWithSegments("/api/v1/subscription/webhooks"))
        {
            await _next(context);
            return;
        }

        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier)
                     ?? context.User.FindFirstValue("sub")
                     ?? context.User.FindFirstValue("id");

        if (string.IsNullOrWhiteSpace(userId) || _redis == null || !_redis.IsConnected)
        {
            await _next(context);
            return;
        }

        try
        {
            var db = _redis.GetDatabase();

            // 1. Check user subscription tier in Redis
            var tierKey = $"tier:{userId}";
            var cachedTier = await db.StringGetAsync(tierKey);
            var isPremium = cachedTier.HasValue && cachedTier.ToString() == "Premium";

            // Premium limit: 2000 req/hr, Free limit: 100 req/hr
            var limit = isPremium ? 2000 : 100;

            // 2. Sliding hourly rate window bucket
            var currentHour = DateTime.UtcNow.ToString("yyyyMMddHH");
            var rateKey = $"rate:user:{userId}:{currentHour}";

            var currentCount = await db.StringIncrementAsync(rateKey);
            if (currentCount == 1)
            {
                await db.KeyExpireAsync(rateKey, TimeSpan.FromHours(1));
            }

            var remaining = Math.Max(0, limit - currentCount);

            context.Response.Headers["X-RateLimit-Limit"] = limit.ToString();
            context.Response.Headers["X-RateLimit-Remaining"] = remaining.ToString();
            context.Response.Headers["X-RateLimit-Tier"] = isPremium ? "Premium" : "Free";

            if (currentCount > limit)
            {
                _logger.LogWarning("Rate limit exceeded for user {UserId} (Tier: {Tier}, Limit: {Limit})",
                    userId, isPremium ? "Premium" : "Free", limit);

                context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                context.Response.ContentType = "application/json";

                var errorResponse = new
                {
                    isSuccess = false,
                    message = $"Rate limit exceeded. Allowed: {limit} requests per hour. Upgrading to Premium lifts the limit to 2000 req/hr.",
                    statusCode = 429,
                    errorCode = "RATE_LIMIT_EXCEEDED",
                    tier = isPremium ? "Premium" : "Free"
                };

                await context.Response.WriteAsync(JsonSerializer.Serialize(errorResponse));
                return;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to apply dynamic rate limit check. Failing open gracefully.");
        }

        await _next(context);
    }
}
