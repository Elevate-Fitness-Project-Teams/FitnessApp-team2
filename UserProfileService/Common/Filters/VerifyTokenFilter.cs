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
        var authServiceUrl = _configuration["AuthServiceUrl"]!;
        client.BaseAddress = new Uri(authServiceUrl);

        try
        {
            //  Send the POST request to the Auth Service
            var requestPayload = new { Token = token };
            var response = await client.PostAsJsonAsync("api/auth/validate-token", requestPayload);

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
        return await next(context);
    }
}

