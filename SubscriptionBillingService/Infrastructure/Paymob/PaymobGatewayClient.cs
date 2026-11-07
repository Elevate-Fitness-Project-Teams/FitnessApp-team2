using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SubscriptionBillingService.Infrastructure.Paymob.Models;

namespace SubscriptionBillingService.Infrastructure.Paymob;

public class PaymobGatewayClient : IPaymobGatewayClient
{
    private readonly HttpClient _httpClient;
    private readonly PaymobOptions _options;
    private readonly ILogger<PaymobGatewayClient> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public PaymobGatewayClient(
        HttpClient httpClient,
        IOptions<PaymobOptions> options,
        ILogger<PaymobGatewayClient> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;

        if (_httpClient.BaseAddress == null)
        {
            _httpClient.BaseAddress = new Uri(_options.BaseUrl);
        }
    }

    public async Task<PaymobIntentionResponse> CreateIntentionAsync(
        PaymobIntentionRequest request,
        CancellationToken cancellationToken = default)
    {
        var url = "/v1/intention/";
        var json = JsonSerializer.Serialize(request, JsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = content
        };
        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Token", _options.SecretKey);

        _logger.LogInformation("Calling Paymob CreateIntention API for SpecialReference: {Ref}, Amount: {Amount}",
            request.SpecialReference, request.Amount);

        var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Paymob CreateIntention failed with status {StatusCode}: {ResponseBody}",
                response.StatusCode, responseBody);
            throw new HttpRequestException($"Paymob CreateIntention failed with status {response.StatusCode}: {responseBody}");
        }

        var result = JsonSerializer.Deserialize<PaymobIntentionResponse>(responseBody, JsonOptions);
        return result ?? throw new InvalidOperationException("Failed to deserialize Paymob intention response.");
    }

    public async Task<PaymobRefundResponse> RefundTransactionAsync(
        PaymobRefundRequest request,
        CancellationToken cancellationToken = default)
    {
        var url = "/api/acceptance/void_refund/refund";
        var json = JsonSerializer.Serialize(request, JsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = content
        };
        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Token", _options.SecretKey);

        _logger.LogInformation("Calling Paymob Refund API for TransactionId: {TxId}, AmountCents: {Amount}",
            request.TransactionId, request.AmountCents);

        var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Paymob Refund failed with status {StatusCode}: {ResponseBody}",
                response.StatusCode, responseBody);
            throw new HttpRequestException($"Paymob Refund failed with status {response.StatusCode}: {responseBody}");
        }

        var result = JsonSerializer.Deserialize<PaymobRefundResponse>(responseBody, JsonOptions);
        return result ?? throw new InvalidOperationException("Failed to deserialize Paymob refund response.");
    }

    public async Task<PaymobTransactionObject> GetTransactionByIdAsync(
        long transactionId,
        CancellationToken cancellationToken = default)
    {
        var url = $"/api/acceptance/transactions/{transactionId}";

        using var httpRequest = new HttpRequestMessage(HttpMethod.Get, url);
        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Token", _options.SecretKey);

        _logger.LogInformation("Calling Paymob Transaction Inquiry API for TransactionId: {TxId}", transactionId);

        var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Paymob GetTransaction failed with status {StatusCode}: {ResponseBody}",
                response.StatusCode, responseBody);
            throw new HttpRequestException($"Paymob GetTransaction failed with status {response.StatusCode}: {responseBody}");
        }

        var result = JsonSerializer.Deserialize<PaymobTransactionObject>(responseBody, JsonOptions);
        return result ?? throw new InvalidOperationException("Failed to deserialize Paymob transaction inquiry response.");
    }
}
