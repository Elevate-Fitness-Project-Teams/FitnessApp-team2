# 📦 Subscription & Billing Service (`SubscriptionBillingService`)

> **Elevate Fitness Architecture & Service Documentation**  
> **Target Framework:** .NET 9.0 (ASP.NET Core Web API)  
> **Persistence:** SQL Server (`SubscriptionBillingDb`) via Entity Framework Core 9  
> **Caching:** Redis Distributed Cache (`StackExchange.Redis`)  
> **Messaging & Outbox:** MassTransit with Entity Framework Core Outbox & RabbitMQ  
> **Payment Gateway:** 100% Native Paymob Accept (Unified Intention API + 20-field HMAC-SHA512 Webhooks + Void/Refund API)  

---

## 1. System Architecture & Topology

```mermaid
flowchart TB
    subgraph ClientLayer["Client Layer"]
        Client["Web / Mobile App"]
    end

    subgraph EdgeLayer["Edge / API Gateway (Port 5000)"]
        Gateway["ApiGateway (Ocelot Reverse Proxy)"]
        RateLimiter["Dynamic Tier Rate Limiter\n(2000 req/hr Premium / 100 req/hr Free)"]
    end

    subgraph BillingService["SubscriptionBillingService (Port 5060 / 8092)"]
        Endpoints["REST API Endpoints\n(Status, Upgrade, Cancel, Refund, Webhook)"]
        MediatR["MediatR Orchestrators\n(IUnitOfWork.ExecuteAsync)"]
        MTOutbox["MassTransit EF Core Outbox\n(OutboxMessage, InboxState)"]
        RefundConsumer["ProcessPaymobRefundConsumer\n(Outbox-Driven Auto & Explicit Refunds)"]
        Sweepers["Background Workers:\n1. SubscriptionExpirationSweeper\n2. PendingRefundSweeper"]
    end

    subgraph PaymobAccept["Paymob Accept Gateway"]
        PaymobIntention["Intention API\nPOST /v1/intention/"]
        PaymobCheckout["Unified Checkout Page\n(Cards, Meeza, Wallets)"]
        PaymobRefund["Refund API\nPOST /api/acceptance/void_refund/refund"]
    end

    subgraph DataAndInfra["Shared Infrastructure"]
        SQL[("SQL Server (Port 1433)\nSubscriptionBillingDb")]
        Redis[("Redis (Port 6379)\nKey: tier:{userId}\nKey: rate:user:{userId}:{hour}")]
        RabbitMQ[["RabbitMQ (Port 5672)\nEvent Bus"]]
    end

    subgraph DownstreamServices["Downstream Consumers"]
        ProfileSvc["UserProfileService\n(Syncs IsPremiumCached = true/false)"]
    end

    Client -->|"1. POST /api/v1/subscription/upgrade"| Gateway
    Gateway --> RateLimiter
    RateLimiter --> Endpoints
    Endpoints --> MediatR
    MediatR -->|"2. Create Intention"| PaymobIntention
    PaymobIntention -->|"client_secret"| MediatR
    MediatR -->|"Returns checkoutUrl"| Client
    Client -->|"3. Completes 3DS Payment"| PaymobCheckout
    PaymobCheckout -->|"4. Callback POST /webhooks/paymob?hmac=..."| Gateway
    Gateway --> Endpoints
    MediatR -->|"5. Atomic Write (UoW)"| SQL
    MediatR -->|"6. Cache tier:{userId} = 'Premium'"| Redis
    MTOutbox -->|"7a. Publish SubscriptionUpgradedEvent"| RabbitMQ
    MTOutbox -->|"7b. Publish ExecutePaymobRefundCommandEvent\n(If duplicate charge detected)"| RabbitMQ
    RabbitMQ --> ProfileSvc
    RabbitMQ --> RefundConsumer
    RefundConsumer -->|"8. POST /api/acceptance/void_refund/refund"| PaymobRefund
    Sweepers --> SQL
```

---

## 2. Domain Model & Database Schema

The service manages subscription lifecycle, payment attempts, immutable billing audit logs, and refund state machines:

```mermaid
erDiagram
    UserSubscriptions {
        Guid Id PK
        Guid UserId UK "UX_UserSubscriptions_UserId"
        string Tier "Free | Premium"
        string Status "Active | Cancelled | Expired | PastDue | Refunded"
        int PlanDurationMonths "1 | 3 | 6 | 12"
        bool AutoRenew "Default true; false on cancel/refund"
        DateTime StartedAtUtc
        DateTime ExpiresAtUtc "Indexed for ExpirationSweeper"
        DateTime CancelledAtUtc "Nullable"
        string LastProviderTransactionId "Nullable"
        byte[] RowVersion "Concurrency Token"
        DateTime CreatedAt
        DateTime UpdatedAt
        bool IsDeleted
    }

    PaymentAttempts {
        Guid Id PK
        Guid UserId "IX_PaymentAttempts_UserId_Status"
        string IdempotencyKey UK "UX_PaymentAttempts_UserId_IdempotencyKey"
        string PlanTier "Premium"
        int DurationMonths "1 | 3 | 6 | 12"
        int AmountCents "e.g. 19900 (199.00 EGP)"
        string Currency "EGP"
        string Status "Initiated | PendingRedirect | Succeeded | Failed | Superseded | DuplicateChargePendingRefund | AutoRefunded | Refunded | Expired"
        string ProviderIntentionId "Nullable"
        string ProviderOrderId "Nullable"
        string ProviderTransactionId "Nullable"
        string ClientSecret "Nullable"
        string CheckoutUrl "Nullable"
        string FailureReason "Nullable"
        DateTime ExpiresAtUtc "30-min window"
        DateTime CompletedAtUtc "Nullable"
        byte[] RowVersion "Concurrency Token"
        DateTime CreatedAt
        DateTime UpdatedAt
        bool IsDeleted
    }

    BillingLogs {
        Guid Id PK
        Guid UserId "IX_BillingLogs_UserId_CreatedAt"
        Guid PaymentAttemptId FK "Links to PaymentAttempts.Id"
        string PlanTier "Premium"
        int DurationMonths "1 | 3 | 6 | 12"
        decimal Amount "e.g. 199.00 or -199.00"
        string Currency "EGP"
        string PaymentProvider "Paymob"
        string ProviderTransactionId UK "UX_BillingLogs_ProviderTransactionId"
        string ProviderIntentionId "Nullable"
        string PaymentStatus "Success | Failed | DuplicateChargeDetected | Refunded"
        string RawPayloadHash "Nullable"
        string FailureReason "Nullable"
        DateTime CreatedAt
        DateTime UpdatedAt
        bool IsDeleted
    }

    RefundRequests {
        Guid Id PK
        Guid UserId "IX_RefundRequests_UserId"
        Guid PaymentAttemptId FK "Links to PaymentAttempts.Id"
        string ProviderTransactionId UK "UX_RefundRequests_ProviderTransactionId"
        string ProviderRefundTransactionId "Nullable (Paymob refund tx id)"
        int AmountCents "Exact cents to reverse"
        string Currency "EGP"
        string Reason "DuplicateActiveSubscription | ExplicitSubscriptionRefund"
        string RefundType "AutoDuplicateRefund | ExplicitSubscriptionRefund"
        string Status "Pending | Refunded | FailedManualIntervention"
        int RetryCount "Default 0"
        string LastError "Nullable"
        DateTime CompletedAtUtc "Nullable"
        byte[] RowVersion "Concurrency Token"
        DateTime CreatedAt
        DateTime UpdatedAt
        bool IsDeleted
    }

    PaymentAttempts ||--o{ BillingLogs : "has audit trail"
    PaymentAttempts ||--o| RefundRequests : "triggers refund"
```

---

## 3. Paymob Native Integration & Atomic Auto-Refund Saga

### 3.1 20-Field Lexicographical HMAC-SHA512 Verification
All incoming transaction webhooks (`POST /api/v1/subscription/webhooks/paymob?hmac=...`) are verified by concatenating these exact 20 properties in alphabetical order (booleans formatted as lowercase `"true"` or `"false"`):
1. `amount_cents`
2. `created_at`
3. `currency`
4. `error_occured`
5. `has_parent_transaction`
6. `id`
7. `integration_id`
8. `is_3d_secure`
9. `is_auth`
10. `is_capture`
11. `is_refunded`
12. `is_standalone_payment`
13. `is_voided`
14. `order.id`
15. `owner`
16. `pending`
17. `source_data.pan`
18. `source_data.sub_type`
19. `source_data.type`
20. `success`

The hash is computed with `HMACSHA512(HmacSecret)` and verified using **`CryptographicOperations.FixedTimeEquals`** to prevent side-channel timing attacks.

### 3.2 Sequence Diagram: Accidental Duplicate Payment & Atomic Auto-Refund

```mermaid
sequenceDiagram
    autonumber
    participant Paymob as Paymob Accept
    participant Webhook as SubscriptionBillingService<br/>(ProcessPaymobWebhookOrchestrator)
    participant UoW as SQL Server<br/>(IUnitOfWork.ExecuteAsync)
    participant Outbox as MassTransit Outbox
    participant Consumer as ProcessPaymobRefundConsumer
    participant PaymobRefund as Paymob Refund API

    Paymob->>Webhook: POST /webhooks/paymob?hmac=... (success = true, obj.id = 88776655)
    Webhook->>Webhook: Validate HMAC-SHA512 (FixedTimeEquals)
    Webhook->>UoW: Begin Transaction (Pessimistic Row Lock)
    Webhook->>UoW: Check UserSubscriptions(UserId)
    Note over Webhook,UoW: User ALREADY has active Premium subscription!
    Webhook->>UoW: Update PaymentAttempts (Status = DuplicateChargePendingRefund)
    Webhook->>UoW: Insert BillingLogs (Status = DuplicateChargeDetected, Tx = 88776655)
    Webhook->>UoW: Insert RefundRequests (Status = Pending, Amount = 19900, Tx = 88776655)
    Webhook->>Outbox: Stage ExecutePaymobRefundCommandEvent
    UoW-->>Webhook: Commit SQL Transaction Atomically
    Webhook-->>Paymob: 200 OK (Webhook Accepted)

    Outbox->>Consumer: Deliver ExecutePaymobRefundCommandEvent
    Consumer->>PaymobRefund: POST /api/acceptance/void_refund/refund<br/>{ transaction_id: 88776655, amount_cents: 19900 }
    PaymobRefund-->>Consumer: 200 OK { id: 99887766, success: true, is_refund: true }

    Consumer->>UoW: Begin Transaction
    Consumer->>UoW: Update RefundRequests (Status = Refunded, RefundTx = 99887766)
    Consumer->>UoW: Update PaymentAttempts (Status = AutoRefunded)
    Consumer->>UoW: Insert BillingLogs (Amount = -199.00, Status = Refunded)
    Consumer->>Outbox: Stage PaymentRefundedEvent
    UoW-->>Consumer: Commit Transaction
```

---

## 4. API Endpoints Reference

### 4.1 Get Subscription Status (Story 8.1)
- **Path:** `GET /api/v1/subscription/status/{userId:guid}`
- **Auth:** Bearer JWT
- **Response (200 OK — Active):**
  ```json
  {
    "data": {
      "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "tier": "Premium",
      "status": "Active",
      "autoRenew": true,
      "expiresAt": "2026-12-07T12:00:00Z"
    },
    "isSuccess": true,
    "message": "Subscription status retrieved successfully.",
    "statusCode": 200
  }
  ```
- **Response (200 OK — No subscription row exists):**
  ```json
  {
    "data": {
      "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "tier": "Free",
      "status": "None",
      "autoRenew": false,
      "expiresAt": null
    },
    "isSuccess": true,
    "statusCode": 200
  }
  ```

### 4.2 Upgrade to Premium (Story 8.2)
- **Path:** `POST /api/v1/subscription/upgrade`
- **Headers:** `Authorization: Bearer <token>`, `Idempotency-Key: <guid>`
- **Request:**
  ```json
  {
    "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "planTier": 1,
    "durationMonths": 1
  }
  ```
- **Response (200 OK — Checkout Session Created):**
  ```json
  {
    "data": {
      "paymentAttemptId": "9b1deb4d-3b7d-4bad-9bdd-2b0d7b3dcb6d",
      "providerIntentionId": "pi_123456",
      "checkoutUrl": "https://accept.paymob.com/unifiedcheckout/?publicKey=pk_test_...&clientSecret=cs_test_...",
      "clientSecret": "cs_test_...",
      "amount": 199.00,
      "currency": "EGP",
      "status": "PendingRedirect",
      "expiresAt": "2026-11-07T15:30:00Z"
    },
    "isSuccess": true
  }
  ```
- **Response (409 Conflict):** User already has active Premium subscription (`SUB_ALREADY_ACTIVE`).

### 4.3 Paymob Webhook Callback
- **Path:** `POST /api/v1/subscription/webhooks/paymob?hmac={hmac}`
- **Auth:** Anonymous (HMAC-SHA512 verified)
- **Response:** `200 OK`

### 4.4 Cancel Subscription (Story 8.3)
- **Path:** `POST /api/v1/subscription/cancel`
- **Auth:** Bearer JWT
- **Request:**
  ```json
  {
    "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
  }
  ```
- **Response (200 OK):** Auto-renewal turned off without truncating `expiresAt`.
- **Response (404 Not Found):** No active subscription to cancel (`RES_NOT_FOUND`).

### 4.5 Explicit Atomic Refund
- **Path:** `POST /api/v1/subscription/refund`
- **Auth:** Bearer JWT
- **Request:**
  ```json
  {
    "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "reason": "Customer requested refund within window."
  }
  ```
- **Response (200 OK):** Immediately revokes subscription, invalidates Redis tier cache, and stages Outbox refund command.

---

## 5. Background Workers (Sweepers)

1. **`SubscriptionExpirationSweeper`:**
   - Runs every 5 minutes (`PeriodicTimer`).
   - Identifies subscriptions where `Status == Active` and `ExpiresAtUtc <= UtcNow`.
   - Transitions `Status = Expired`, deletes `tier:{userId}` from Redis, and stages `SubscriptionExpiredEvent`.
2. **`PendingRefundSweeper`:**
   - Runs every 5 minutes.
   - Dispatches any `Pending` refund requests older than 2 minutes to ensure no transient failure prevents a customer refund.

---

## 6. Running & Verification Commands

```bash
# Build the project
dotnet build SubscriptionBillingService/SubscriptionBillingService.csproj

# Run all test suites
dotnet test SubscriptionBillingService/SubscriptionBillingService.csproj -c Release

# Run entire solution test suite
dotnet test FitnessApp.slnx -c Release --no-restore --logger "trx;LogFileName=test_results.trx" --collect:"XPlat Code Coverage"
```
