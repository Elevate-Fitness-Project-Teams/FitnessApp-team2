using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SubscriptionBillingService.Common;
using SubscriptionBillingService.Common.Database;
using SubscriptionBillingService.Common.Errors;
using SubscriptionBillingService.Domain.Catalog;
using SubscriptionBillingService.Domain.Entities;
using SubscriptionBillingService.Domain.Enums;
using SubscriptionBillingService.Features.Subscriptions.Commands.UpgradeSubscription;
using SubscriptionBillingService.Infrastructure.Paymob;
using SubscriptionBillingService.Infrastructure.Paymob.Models;

namespace SubscriptionBillingService.Features.Subscriptions.Orchestrators;

public class UpgradeSubscriptionOrchestrator : IRequestHandler<UpgradeSubscriptionCommand, Result<UpgradeSubscriptionResponse>>
{
    private readonly IGenericRepository<UserSubscription> _subscriptionRepo;
    private readonly IGenericRepository<PaymentAttempt> _attemptRepo;
    private readonly IGenericRepository<BillingLog> _billingLogRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPaymobGatewayClient _paymobClient;
    private readonly PaymobOptions _paymobOptions;
    private readonly ILogger<UpgradeSubscriptionOrchestrator> _logger;

    public UpgradeSubscriptionOrchestrator(
        IGenericRepository<UserSubscription> subscriptionRepo,
        IGenericRepository<PaymentAttempt> attemptRepo,
        IGenericRepository<BillingLog> billingLogRepo,
        IUnitOfWork unitOfWork,
        IPaymobGatewayClient paymobClient,
        IOptions<PaymobOptions> paymobOptions,
        ILogger<UpgradeSubscriptionOrchestrator> logger)
    {
        _subscriptionRepo = subscriptionRepo;
        _attemptRepo = attemptRepo;
        _billingLogRepo = billingLogRepo;
        _unitOfWork = unitOfWork;
        _paymobClient = paymobClient;
        _paymobOptions = paymobOptions.Value;
        _logger = logger;
    }

    public async Task<Result<UpgradeSubscriptionResponse>> Handle(
        UpgradeSubscriptionCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Active subscription guard
        var activeSub = await _subscriptionRepo.FirstOrDefaultAsync(
            s => s.UserId == request.UserId, cancellationToken);

        if (activeSub != null &&
            activeSub.Tier == SubscriptionTier.Premium &&
            activeSub.Status == SubscriptionStatus.Active &&
            activeSub.ExpiresAtUtc.HasValue &&
            activeSub.ExpiresAtUtc.Value > DateTime.UtcNow)
        {
            _logger.LogWarning("User {UserId} already has active Premium subscription expiring at {ExpiresAt}",
                request.UserId, activeSub.ExpiresAtUtc);
            return Result<UpgradeSubscriptionResponse>.Failure(SubscriptionErrors.AlreadyActive());
        }

        // 2. Check Plan Catalog
        var plan = SubscriptionPlanCatalog.GetPlanOrThrow(request.PlanTier, request.DurationMonths);

        var idempotencyKey = string.IsNullOrWhiteSpace(request.IdempotencyKey)
            ? $"sub-upg-{request.UserId}-{request.PlanTier}-{request.DurationMonths}-{DateTime.UtcNow:yyyyMMddHHmm}"
            : request.IdempotencyKey;

        // Check for existing attempt with same idempotency key or active reusable pending attempt
        var existingAttempt = await _attemptRepo.FirstOrDefaultAsync(
            a => a.UserId == request.UserId && a.IdempotencyKey == idempotencyKey, cancellationToken);

        if (existingAttempt != null &&
            existingAttempt.Status == PaymentAttemptStatus.PendingRedirect &&
            existingAttempt.ExpiresAtUtc > DateTime.UtcNow &&
            !string.IsNullOrEmpty(existingAttempt.CheckoutUrl))
        {
            _logger.LogInformation("Reusing existing pending checkout attempt {AttemptId} for user {UserId}",
                existingAttempt.Id, request.UserId);

            return Result<UpgradeSubscriptionResponse>.Success(new UpgradeSubscriptionResponse(
                existingAttempt.Id,
                existingAttempt.ProviderIntentionId ?? string.Empty,
                existingAttempt.CheckoutUrl,
                existingAttempt.ClientSecret ?? string.Empty,
                existingAttempt.AmountCents / 100m,
                existingAttempt.Currency,
                existingAttempt.Status.ToString(),
                existingAttempt.ExpiresAtUtc));
        }

        // 3. Persist Initial PaymentAttempt (Initiated)
        var attempt = new PaymentAttempt
        {
            UserId = request.UserId,
            IdempotencyKey = idempotencyKey,
            PlanTier = request.PlanTier,
            DurationMonths = request.DurationMonths,
            AmountCents = plan.AmountCents,
            Currency = plan.Currency,
            Status = PaymentAttemptStatus.Initiated,
            ExpiresAtUtc = DateTime.UtcNow.AddMinutes(30)
        };

        await _attemptRepo.AddAsync(attempt, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // 4. Call Paymob Intention API outside DB transaction
        PaymobIntentionResponse intentionResponse;
        try
        {
            var intentionRequest = new PaymobIntentionRequest
            {
                Amount = plan.AmountCents,
                Currency = plan.Currency,
                PaymentMethods = new List<int> { _paymobOptions.CardIntegrationId },
                Items = new List<PaymobIntentionItem>
                {
                    new()
                    {
                        Name = plan.Name,
                        Amount = plan.AmountCents,
                        Description = plan.Description,
                        Quantity = 1
                    }
                },
                SpecialReference = attempt.Id.ToString(),
                BillingData = new PaymobBillingData
                {
                    FirstName = "Elevate",
                    LastName = "Member",
                    Email = "athlete@elevatefitness.com",
                    PhoneNumber = "+201000000000",
                    Country = "EG"
                },
                Extras = new Dictionary<string, string>
                {
                    ["payment_attempt_id"] = attempt.Id.ToString(),
                    ["user_id"] = request.UserId.ToString(),
                    ["plan_tier"] = request.PlanTier.ToString(),
                    ["duration_months"] = request.DurationMonths.ToString()
                }
            };

            intentionResponse = await _paymobClient.CreateIntentionAsync(intentionRequest, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create Paymob intention for attempt {AttemptId}", attempt.Id);

            attempt.Status = PaymentAttemptStatus.Failed;
            attempt.FailureReason = ex.Message;
            _attemptRepo.Update(attempt);

            var failureLog = new BillingLog
            {
                UserId = request.UserId,
                PaymentAttemptId = attempt.Id,
                PlanTier = request.PlanTier,
                DurationMonths = request.DurationMonths,
                Amount = plan.Amount,
                Currency = plan.Currency,
                PaymentProvider = "Paymob",
                ProviderTransactionId = $"failed_{attempt.Id:N}",
                PaymentStatus = BillingPaymentStatus.Failed,
                FailureReason = ex.Message
            };
            await _billingLogRepo.AddAsync(failureLog, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<UpgradeSubscriptionResponse>.Failure(SubscriptionErrors.ProviderError(ex.Message));
        }

        // 5. Update attempt with Paymob checkout details
        attempt.Status = PaymentAttemptStatus.PendingRedirect;
        attempt.ProviderIntentionId = intentionResponse.Id;
        attempt.ClientSecret = intentionResponse.ClientSecret;
        attempt.CheckoutUrl = $"https://accept.paymob.com/unifiedcheckout/?publicKey={_paymobOptions.PublicKey}&clientSecret={intentionResponse.ClientSecret}";

        _attemptRepo.Update(attempt);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<UpgradeSubscriptionResponse>.Success(new UpgradeSubscriptionResponse(
            attempt.Id,
            intentionResponse.Id,
            attempt.CheckoutUrl,
            intentionResponse.ClientSecret,
            plan.Amount,
            plan.Currency,
            attempt.Status.ToString(),
            attempt.ExpiresAtUtc));
    }
}
