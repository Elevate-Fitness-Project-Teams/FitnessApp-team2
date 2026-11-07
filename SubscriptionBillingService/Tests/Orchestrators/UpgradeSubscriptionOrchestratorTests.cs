using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using SubscriptionBillingService.Common.Database;
using SubscriptionBillingService.Domain.Entities;
using SubscriptionBillingService.Domain.Enums;
using SubscriptionBillingService.Features.Subscriptions.Commands.UpgradeSubscription;
using SubscriptionBillingService.Features.Subscriptions.Orchestrators;
using SubscriptionBillingService.Infrastructure.Paymob;
using SubscriptionBillingService.Infrastructure.Paymob.Models;
using Xunit;

namespace SubscriptionBillingService.Tests.Orchestrators;

public class UpgradeSubscriptionOrchestratorTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly SubscriptionDbContext _dbContext;
    private readonly IGenericRepository<UserSubscription> _subscriptionRepo;
    private readonly IGenericRepository<PaymentAttempt> _attemptRepo;
    private readonly IGenericRepository<BillingLog> _billingLogRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOptions<PaymobOptions> _paymobOptions;

    public UpgradeSubscriptionOrchestratorTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<SubscriptionDbContext>()
            .UseSqlite(_connection)
            .Options;

        _dbContext = new SubscriptionDbContext(options);
        _dbContext.Database.EnsureCreated();

        _subscriptionRepo = new GenericRepository<UserSubscription>(_dbContext);
        _attemptRepo = new GenericRepository<PaymentAttempt>(_dbContext);
        _billingLogRepo = new GenericRepository<BillingLog>(_dbContext);
        _unitOfWork = new UnitOfWork(_dbContext);

        _paymobOptions = Options.Create(new PaymobOptions
        {
            PublicKey = "test_pk_123",
            SecretKey = "test_sk_123",
            CardIntegrationId = 112233
        });
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        _connection.Close();
        _connection.Dispose();
    }

    private class MockPaymobClient : IPaymobGatewayClient
    {
        public bool ShouldThrow { get; set; }

        public Task<PaymobIntentionResponse> CreateIntentionAsync(PaymobIntentionRequest request, CancellationToken cancellationToken = default)
        {
            if (ShouldThrow)
            {
                throw new HttpRequestException("Paymob connection timeout");
            }

            return Task.FromResult(new PaymobIntentionResponse
            {
                Id = "pi_mock_123",
                ClientSecret = "cs_mock_456",
                Amount = request.Amount,
                Currency = request.Currency,
                SpecialReference = request.SpecialReference
            });
        }

        public Task<PaymobRefundResponse> RefundTransactionAsync(PaymobRefundRequest request, CancellationToken cancellationToken = default)
            => throw new NotImplementedException();

        public Task<PaymobTransactionObject> GetTransactionByIdAsync(long transactionId, CancellationToken cancellationToken = default)
            => throw new NotImplementedException();
    }

    [Fact]
    public async Task Handle_WhenUserAlreadyHasActivePremium_ReturnsConflictAlreadyActive()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var activeSub = new UserSubscription
        {
            UserId = userId,
            Tier = SubscriptionTier.Premium,
            Status = SubscriptionStatus.Active,
            ExpiresAtUtc = DateTime.UtcNow.AddMonths(1)
        };
        await _subscriptionRepo.AddAsync(activeSub);
        await _dbContext.SaveChangesAsync();

        var orchestrator = new UpgradeSubscriptionOrchestrator(
            _subscriptionRepo, _attemptRepo, _billingLogRepo, _unitOfWork,
            new MockPaymobClient(), _paymobOptions, NullLogger<UpgradeSubscriptionOrchestrator>.Instance);

        // Act
        var result = await orchestrator.Handle(new UpgradeSubscriptionCommand(userId, SubscriptionTier.Premium, 1), CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("SUB_ALREADY_ACTIVE", result.Error.Code);
    }

    [Fact]
    public async Task Handle_WhenUpgradeInitiated_CreatesAttemptAndReturnsCheckoutUrl()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var orchestrator = new UpgradeSubscriptionOrchestrator(
            _subscriptionRepo, _attemptRepo, _billingLogRepo, _unitOfWork,
            new MockPaymobClient(), _paymobOptions, NullLogger<UpgradeSubscriptionOrchestrator>.Instance);

        // Act
        var result = await orchestrator.Handle(
            new UpgradeSubscriptionCommand(userId, SubscriptionTier.Premium, 1, "idemp-key-1"),
            CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("pi_mock_123", result.Value.ProviderIntentionId);
        Assert.Equal("cs_mock_456", result.Value.ClientSecret);
        Assert.Equal(199.00m, result.Value.Amount);
        Assert.Equal("EGP", result.Value.Currency);
        Assert.Contains("cs_mock_456", result.Value.CheckoutUrl);

        var attempt = await _attemptRepo.GetByIdAsync(result.Value.PaymentAttemptId);
        Assert.NotNull(attempt);
        Assert.Equal(PaymentAttemptStatus.PendingRedirect, attempt.Status);
    }

    [Fact]
    public async Task Handle_WhenPaymobFails_MarksAttemptFailedAndLogsBilling()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var orchestrator = new UpgradeSubscriptionOrchestrator(
            _subscriptionRepo, _attemptRepo, _billingLogRepo, _unitOfWork,
            new MockPaymobClient { ShouldThrow = true }, _paymobOptions, NullLogger<UpgradeSubscriptionOrchestrator>.Instance);

        // Act
        var result = await orchestrator.Handle(
            new UpgradeSubscriptionCommand(userId, SubscriptionTier.Premium, 1, "idemp-key-fail"),
            CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("PAY_PROVIDER_ERROR", result.Error.Code);

        var attempt = await _attemptRepo.FirstOrDefaultAsync(a => a.UserId == userId);
        Assert.NotNull(attempt);
        Assert.Equal(PaymentAttemptStatus.Failed, attempt.Status);

        var log = await _billingLogRepo.FirstOrDefaultAsync(b => b.UserId == userId);
        Assert.NotNull(log);
        Assert.Equal(BillingPaymentStatus.Failed, log.PaymentStatus);
    }
}
