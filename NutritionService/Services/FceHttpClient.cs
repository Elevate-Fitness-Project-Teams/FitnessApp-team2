using System.Net.Http.Headers;
using System.Text.Json;
using NutritionService.Services.Dtos;

namespace NutritionService.Services;

public class FceHttpClient : IFceHttpClient
{
    private readonly HttpClient _httpClient;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<FceHttpClient> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public FceHttpClient(
        IHttpClientFactory httpClientFactory,
        IHttpContextAccessor httpContextAccessor,
        ILogger<FceHttpClient> logger)
    {
        _httpClient = httpClientFactory.CreateClient("FitnessCalculationService");
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public async Task<UserCalorieTargetDto?> GetUserMetricsAsync(Guid userId, CancellationToken cancellationToken)
    {
        // Forward the incoming Authorization header to the FCE service
        var authHeader = _httpContextAccessor.HttpContext?.Request.Headers.Authorization.ToString();
        if (!string.IsNullOrEmpty(authHeader))
        {
            _httpClient.DefaultRequestHeaders.Authorization = AuthenticationHeaderValue.Parse(authHeader);
        }

        try
        {
            var response = await _httpClient.GetAsync($"/api/v1/fitness/metrics/{userId}", cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "FCE service returned {StatusCode} for User {UserId}",
                    response.StatusCode, userId);
                return null;
            }

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var apiResponse = JsonSerializer.Deserialize<FceApiResponse<FceUserMetricsData>>(content, JsonOptions);

            if (apiResponse is null || !apiResponse.IsSuccess || apiResponse.Data is null)
            {
                return null;
            }

            return new UserCalorieTargetDto
            {
                CalorieTarget = apiResponse.Data.CalorieTarget
            };
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP call to FCE service failed for User {UserId}", userId);
            throw;
        }
    }

    /// <summary>
    /// Mirrors the FCE service's ApiResponse envelope for deserialization.
    /// </summary>
    private class FceApiResponse<T>
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
        public IEnumerable<string> Errors { get; set; } = Array.Empty<string>();
        public int StatusCode { get; set; }
    }

    /// <summary>
    /// Mirrors the FCE service's GetUserMetricsResponse for deserialization.
    /// </summary>
    private class FceUserMetricsData
    {
        public string UserId { get; set; } = string.Empty;
        public double Bmr { get; set; }
        public double Tdee { get; set; }
        public double CalorieTarget { get; set; }
        public DateTime CalculatedAt { get; set; }
    }
}
