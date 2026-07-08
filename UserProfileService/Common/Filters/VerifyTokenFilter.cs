using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace UserProfileService.Common.Filters;

public class VerifyTokenEndpointFilter : IEndpointFilter
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public VerifyTokenEndpointFilter(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        //  Extract the token
        var authHeader = context.HttpContext.Request.Headers["Authorization"].FirstOrDefault();
        var token = authHeader?.Split(" ").Last();

        if (string.IsNullOrWhiteSpace(token))
        {
            return Results.Json(new { Message = "Authorization token is missing." }, statusCode: StatusCodes.Status401Unauthorized);
        }

        var client = _httpClientFactory.CreateClient();
        var authServiceUrl = _configuration["ServiceUrls:AuthenticationService"]!;
        client.BaseAddress = new Uri(authServiceUrl);

        try
        {
            //  Send the POST request to the Auth Service
            var requestPayload = new { Token = token };
            var response = await client.PostAsJsonAsync("api/v1/auth/validate-token", requestPayload);

            if (!response.IsSuccessStatusCode)
            {
                // Token is invalid, block request
                return Results.Json(new { Message = "Token is invalid or expired." }, statusCode: StatusCodes.Status401Unauthorized);
            }
        }
        catch (HttpRequestException)
        {
            // Auth Service is down
            return Results.Json(new { Message = "Authentication service is currently unavailable." }, statusCode: StatusCodes.Status503ServiceUnavailable);
        }
        // 4. Token is valid! Now let's extract the claims so the endpoint can use them.
        var handler = new JwtSecurityTokenHandler();
        if (handler.CanReadToken(token))
        {
            var jwtToken = handler.ReadJwtToken(token);

            // Build claims explicitly from the JWT payload to avoid claim type mapping issues
            var claims = new List<Claim>();

            // Add the sub claim with both possible type names to ensure compatibility
            if (!string.IsNullOrEmpty(jwtToken.Subject))
            {
                claims.Add(new Claim(JwtRegisteredClaimNames.Sub, jwtToken.Subject));
                claims.Add(new Claim(ClaimTypes.NameIdentifier, jwtToken.Subject));
            }

            // Add all other claims from the token
            foreach (var claim in jwtToken.Claims)
            {
                if (claim.Type != JwtRegisteredClaimNames.Sub &&
                    claim.Type != ClaimTypes.NameIdentifier &&
                    claim.Type != "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")
                {
                    claims.Add(claim);
                }
            }

            var identity = new ClaimsIdentity(claims, "CustomAuth");

            // Attach the user identity to the current HttpContext
            context.HttpContext.User = new ClaimsPrincipal(identity);
        }
        return await next(context);
    }
}

