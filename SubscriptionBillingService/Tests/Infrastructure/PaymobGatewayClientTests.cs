using System.Net;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using SubscriptionBillingService.Infrastructure.Paymob;
using SubscriptionBillingService.Infrastructure.Paymob.Models;
using Xunit;

namespace SubscriptionBillingService.Tests.Infrastructure;

public class PaymobGatewayClientTests
{
    private class TestHttpMessageHandler : HttpMessageHandler
    {
        public Func<HttpRequestMessage, HttpResponseMessage>? Handler { get; set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (Handler == null)
            {
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
            }
            return Task.FromResult(Handler(request));
        }
    }

    [Fact]
    public async Task CreateIntentionAsync_WhenSuccessful_ReturnsIntentionResponse()
    {
        // Arrange
        var handler = new TestHttpMessageHandler
        {
            Handler = req =>
            {
                Assert.Equal(HttpMethod.Post, req.Method);
                Assert.Equal("/v1/intention/", req.RequestUri!.AbsolutePath);
                Assert.Equal("Token", req.Headers.Authorization!.Scheme);
                Assert.Equal("test_secret_key", req.Headers.Authorization!.Parameter);

                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(
                        "{\"id\": \"pi_12345\", \"client_secret\": \"cs_abc123\", \"amount\": 19900, \"currency\": \"EGP\", \"special_reference\": \"ref-1\"}")
                };
            }
        };

        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://accept.paymob.com")
        };

        var options = Options.Create(new PaymobOptions
        {
            BaseUrl = "https://accept.paymob.com",
            SecretKey = "test_secret_key"
        });

        var client = new PaymobGatewayClient(httpClient, options, NullLogger<PaymobGatewayClient>.Instance);

        // Act
        var result = await client.CreateIntentionAsync(new PaymobIntentionRequest
        {
            Amount = 19900,
            Currency = "EGP",
            SpecialReference = "ref-1"
        });

        // Assert
        Assert.NotNull(result);
        Assert.Equal("pi_12345", result.Id);
        Assert.Equal("cs_abc123", result.ClientSecret);
        Assert.Equal(19900, result.Amount);
    }

    [Fact]
    public async Task RefundTransactionAsync_WhenSuccessful_ReturnsRefundResponse()
    {
        // Arrange
        var handler = new TestHttpMessageHandler
        {
            Handler = req =>
            {
                Assert.Equal(HttpMethod.Post, req.Method);
                Assert.Equal("/api/acceptance/void_refund/refund", req.RequestUri!.AbsolutePath);
                Assert.Equal("Token", req.Headers.Authorization!.Scheme);

                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(
                        "{\"id\": 98769999, \"success\": true, \"is_refund\": true, \"amount_cents\": 19900, \"parent_transaction\": 987654321}")
                };
            }
        };

        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://accept.paymob.com")
        };

        var options = Options.Create(new PaymobOptions
        {
            BaseUrl = "https://accept.paymob.com",
            SecretKey = "test_secret_key"
        });

        var client = new PaymobGatewayClient(httpClient, options, NullLogger<PaymobGatewayClient>.Instance);

        // Act
        var result = await client.RefundTransactionAsync(new PaymobRefundRequest
        {
            TransactionId = 987654321,
            AmountCents = 19900
        });

        // Assert
        Assert.NotNull(result);
        Assert.Equal(98769999, result.Id);
        Assert.True(result.Success);
        Assert.True(result.IsRefund);
        Assert.Equal(987654321, result.ParentTransaction);
    }

    [Fact]
    public async Task GetTransactionByIdAsync_WhenSuccessful_ReturnsTransaction()
    {
        // Arrange
        var handler = new TestHttpMessageHandler
        {
            Handler = req =>
            {
                Assert.Equal(HttpMethod.Get, req.Method);
                Assert.Equal("/api/acceptance/transactions/987654321", req.RequestUri!.AbsolutePath);

                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(
                        "{\"id\": 987654321, \"amount_cents\": 19900, \"currency\": \"EGP\", \"success\": true}")
                };
            }
        };

        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://accept.paymob.com")
        };

        var options = Options.Create(new PaymobOptions
        {
            BaseUrl = "https://accept.paymob.com",
            SecretKey = "test_secret_key"
        });

        var client = new PaymobGatewayClient(httpClient, options, NullLogger<PaymobGatewayClient>.Instance);

        // Act
        var result = await client.GetTransactionByIdAsync(987654321);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(987654321, result.Id);
        Assert.True(result.Success);
        Assert.Equal(19900, result.AmountCents);
    }
}
