<div align="center">

<img src="./assets/logo.svg" alt="NextRep Logo" width="340" />

# NextRep — Cloud-Native Microservices Fitness, Nutrition & Fintech Billing Platform

**An Enterprise-Grade Distributed System Engineered with .NET 9, Vertical Slice Architecture, CQRS, MassTransit Transactional Outbox, gRPC (HTTP/2), Redis 7, Ocelot API Gateway, RS256 Asymmetric JWTs with Sub-Millisecond Hybrid Token Revocation, and Native Paymob Payment Processing**

[![.NET 9.0](https://img.shields.io/badge/.NET-9.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![Architecture](https://img.shields.io/badge/Architecture-Microservices%20%7C%20Vertical%20Slice%20%7C%20CQRS-00ADD8?style=for-the-badge)](./FitnessApp.slnx)
[![Messaging](https://img.shields.io/badge/Event%20Bus-RabbitMQ%20%2B%20MassTransit%20Outbox-FF6600?style=for-the-badge&logo=rabbitmq&logoColor=white)](./docker-compose.yml)
[![RPC](https://img.shields.io/badge/Sync%20RPC-gRPC%20(Protobuf%20HTTP%2F2)-244C5A?style=for-the-badge&logo=google&logoColor=white)](./docker-compose.yml)
[![Cache & Security](https://img.shields.io/badge/Redis%207-MGET%20Token%20Denylist%20%2B%20Cache-DC382D?style=for-the-badge&logo=redis&logoColor=white)](./ApiGateway/Middleware/TokenRevocationMiddleware.cs)
[![Database](https://img.shields.io/badge/Database-SQL%20Server%202022%20(7%20Isolated%20DBs)-CC2927?style=for-the-badge&logo=microsoftsqlserver&logoColor=white)](./docker-compose.yml)
[![Payments](https://img.shields.io/badge/Payments-Paymob%20Intention%20API%20%2B%20HMAC--SHA512-0052FF?style=for-the-badge)](./SubscriptionBillingService/README.md)
[![Tests](https://img.shields.io/badge/Tests-225%20Passed%20(0%20Failed)-2EA44F?style=for-the-badge&logo=githubactions&logoColor=white)](./.github/workflows/production.yml)

</div>

---

## Table of Contents

1. [Platform Capabilities & Product Features](#1-platform-capabilities--product-features)
2. [Business Workflows & Domain State Machines](#2-business-workflows--domain-state-machines)
   - [2.1 End-to-End Athlete Journey (6-Stage Lifecycle Graph)](#21-end-to-end-athlete-journey-6-stage-lifecycle-graph)
   - [2.2 Clinical Metabolic Engine & 15-Plan Prescription Decision Graph](#22-clinical-metabolic-engine--15-plan-prescription-decision-graph)
   - [2.3 Live Workout Session, Habit Streak & Gamification State Machine](#23-live-workout-session-habit-streak--gamification-state-machine)
   - [2.4 Subscription Billing, Renewal & Auto-Refund Lifecycle State Machine](#24-subscription-billing-renewal--auto-refund-lifecycle-state-machine)
3. [Technical & Distributed Systems Architecture Graphs](#3-technical--distributed-systems-architecture-graphs)
   - [3.1 High-Level Cloud-Native System Topology](#31-high-level-cloud-native-system-topology)
   - [3.2 API Gateway Edge Security & Dynamic Rate-Limiting Pipeline](#32-api-gateway-edge-security--dynamic-rate-limiting-pipeline)
   - [3.3 Internal Microservice Architecture (Vertical Slice + CQRS + Outbox)](#33-internal-microservice-architecture-vertical-slice--cqrs--outbox)
   - [3.4 Distributed Redis Keyspace & Multi-Layer Caching Topology](#34-distributed-redis-keyspace--multi-layer-caching-topology)
4. [Inter-Service Communication Graphs (RabbitMQ & gRPC)](#4-inter-service-communication-graphs-rabbitmq--grpc)
   - [4.1 Asynchronous Event-Driven Choreography (RabbitMQ + MassTransit Outbox/Inbox)](#41-asynchronous-event-driven-choreography-rabbitmq--masstransit-outboxinbox)
   - [4.2 Synchronous High-Performance RPC Mesh (gRPC over HTTP/2 + Polly)](#42-synchronous-high-performance-rpc-mesh-grpc-over-http2--polly)
5. [Hard Distributed Systems & Security Engineering Challenges Solved](#5-hard-distributed-systems--security-engineering-challenges-solved)
   - [5.1 RS256 Asymmetric JWTs & Sub-Millisecond (`< 0.3 ms`) Hybrid Redis Token Revocation](#51-rs256-asymmetric-jwts--sub-millisecond--03-ms-hybrid-redis-token-revocation)
   - [5.2 RFC 6819 Refresh Token Reuse / Theft Detection & Multi-Device Session Cutoffs](#52-rfc-6819-refresh-token-reuse--theft-detection--multi-device-session-cutoffs)
   - [5.3 Native Paymob Integration, Constant-Time HMAC-SHA512 & Atomic Auto-Refunds on Race Conditions](#53-native-paymob-integration-constant-time-hmac-sha512--atomic-auto-refunds-on-race-conditions)
   - [5.4 Eliminating Dual-Write Bugs via the MassTransit EF Core Transactional Outbox](#54-eliminating-dual-write-bugs-via-the-masstransit-ef-core-transactional-outbox)
   - [5.5 Dynamic Subscription-Tier Rate Limiting at the Edge (`ApiGateway`)](#55-dynamic-subscription-tier-rate-limiting-at-the-edge-apigateway)
   - [5.6 Decoupling External gRPC Network I/O from SQL Server Transactions](#56-decoupling-external-grpc-network-io-from-sql-server-transactions)
6. [Database-per-Service SQL Server Architecture (Complete ER Diagrams)](#6-database-per-service-sql-server-architecture-complete-er-diagrams)
7. [CI/CD Quality Gate & Container Delivery Pipeline](#7-cicd-quality-gate--container-delivery-pipeline)
8. [Microservice Catalog & Service-Specific Documentation](#8-microservice-catalog--service-specific-documentation)
9. [Getting Started (Docker Compose & Local Test Suite)](#9-getting-started-docker-compose--local-test-suite)

---

## 1. Platform Capabilities & Product Features

**NextRep** is a full-lifecycle digital fitness, metabolic prescription, nutrition planning, progress analytics, and subscription billing platform. It is designed to take an athlete from initial registration and biometric onboarding to daily macro-aligned meal recommendations, live workout tracking, habit streak gamification, and premium subscription management.

```mermaid
flowchart LR
    Platform["NextRep Platform"]

    Platform --> Cap1["1. Identity & Multi-Device Security<br/>- RS256 15m JWT + 7d Refresh Rotation<br/>- 2-Step Email OTP & Brute-Force Guard<br/>- Single-Device & All-Device Instant Logout<br/>- RFC 6819 Token Theft Lockdown"]
    Platform --> Cap2["2. Biometric Profile & Preferences<br/>- Height, Weight, Age, Gender Tracking<br/>- 5 Activity Tiers & 5 Fitness Goals<br/>- Metric/Imperial Units & Theme Settings<br/>- Avatar Upload & Privacy Controls"]
    Platform --> Cap3["3. Clinical Metabolic Engine (FCE)<br/>- Mifflin-St Jeor BMR & TDEE Calculation<br/>- Goal-Adjusted Daily Calorie Targets<br/>- Intensity Tier Classification (Weak/Normal/Hard)<br/>- Automatic 15-Plan Matrix Assignment"]
    Platform --> Cap4["4. Smart Nutrition & Meal Engine<br/>- Multi-Day Structured Meal Plans<br/>- Full Macro, Fiber & Ingredient Breakdowns<br/>- Allergen Severity & Dietary Tag Filtering<br/>- gRPC Calorie-Matched Recommendations"]
    Platform --> Cap5["5. Training & Live Workout Sessions<br/>- Multi-Week Programs & Exercise Catalog<br/>- Sets, Reps, Rest & Video Prescriptions<br/>- Live Session Conflict Guard (InProgress)<br/>- gRPC Cross-Service Session Completion"]
    Platform --> Cap6["6. Analytics, Streaks & Gamification<br/>- UTC Midnight Habit Streak Engine<br/>- Read-Time Streak Decay Evaluation<br/>- Milestone Trophy & Badge Unlocks<br/>- Week/Month/Year Trend Dashboards"]
    Platform --> Cap7["7. Fintech Billing & Edge QoS<br/>- Paymob 3DS Unified Intention Checkout<br/>- 20-Field HMAC-SHA512 Webhook Validation<br/>- Atomic Race-Condition Auto-Refunds<br/>- Dynamic 2,000 vs 100 req/hr Rate Limits"]
```

### Comprehensive Product Feature Matrix

| Domain Capability | User-Facing Product Features | Underlying Engineering & Domain Rules |
| :--- | :--- | :--- |
| **Identity, Authentication & Session Control** | - Account Registration with 6-Digit Email OTP Confirmation<br/>- Secure Login with Onboarding & Subscription State Enrichment<br/>- Seamless Background Session Renewal (Refresh Token Rotation)<br/>- 1-Step & 2-Step Password Reset via Verified OTP<br/>- Authenticated Password Change<br/>- **Single-Device Logout** vs. **"Logout All Devices"** | - **RS256 (2048-bit RSA)** asymmetric JWT signing (`15m` TTL)<br/>- **5-Strike Account Lockout** (`15m` cooldown) + **5-Attempt OTP Brute-Force Guard**<br/>- **Redis `MGET` Denylist (`< 0.3 ms`)** checking `auth:revoked:jti:{jti}` and `auth:revoked-before:{userId}`<br/>- **RFC 6819 Refresh Token Theft Detection** revoking all user sessions on replay of a rotated token |
| **Biometric Onboarding & User Preferences** | - Personal Profile Management (Name, Email, Phone, Avatar Image)<br/>- Biometric Onboarding (Age `16–100`, Gender, Height `140–220 cm`, Weight `40–200 kg`)<br/>- Localization (`en`/`ar`), UI Theme (`light`/`dark`), Measurement Units (`kg`/`cm`/`km`)<br/>- Granular Notification & Privacy Toggles | - Autonomous profile provisioning via `UserRegisteredEvent`<br/>- Cached premium badge synchronization (`IsPremiumCached`) via `SubscriptionUpgradedEvent` & `SubscriptionExpiredEvent`<br/>- Exposes synchronous `ProfileGrpc` server for sub-millisecond onboarding status checks |
| **Metabolic Calculation & Plan Prescription (FCE)** | - Clinical **Basal Metabolic Rate (BMR)** & **Total Daily Energy Expenditure (TDEE)**<br/>- 5 Activity Tiers: `Rookie (1.20)` to `TrueBeast (1.90)`<br/>- 5 Fitness Goals: `Lose Weight (-500 kcal)`, `Get Fitter (0)`, `Gain Weight (+300)`, `Gain More Flexible (+150)`, `Learn the Basic (0)`<br/>- Automatic Workout Plan Prescription across **15 specialized programs** (`LW-W` through `LTB-H`)<br/>- Immutable Plan Assignment History & Audit Trail | - Pure deterministic [`MetabolicCalculator`](./FitnessCalculationService/Domain/Services/MetabolicCalculator.cs) implementing the **Mifflin-St Jeor Equation**<br/>- Classifies daily intake into `Weak (<= 1800 kcal)`, `Normal (1800.01–2500 kcal)`, or `Hard (> 2500 kcal)`<br/>- Event-driven recalculation on `WeightUpdatedEvent` with monotonic watermark guards (`LastWeightEventAt`) to ignore out-of-order messages |
| **Nutrition & Macro Recommendation Engine** | - Paginated Multi-Day Meal Plan Catalog<br/>- Calorie-Targeted Meal Plan Discovery<br/>- Rich Meal Details: Calories, Protein, Carbs, Fats, Fiber, Prep/Cook Time, Step-by-Step Instructions<br/>- Allergen Warnings with Severity Levels & Dietary Tags (`Keto`, `Vegan`, `High-Protein`)<br/>- **Personalized Meal Recommendations** filtered by Meal Type (`Breakfast`, `Lunch`, `Dinner`, `Snack`), Max Calories, and Min Protein | - Real-time synchronous gRPC call (`CalorieTargetService.GetUserMetrics`) to `FitnessCalculationService` to fetch the athlete's exact daily calorie budget<br/>- Protected by **Polly Exponential Backoff Retry + Circuit Breaker** policies (`GrpcPolicies.cs`) |
| **Workout Catalog & Live Session Tracking** | - Multi-Week Structured Workout Plans (`Beginner`, `Intermediate`, `Advanced`)<br/>- Category-Based Workout Filtering (`Strength`, `Cardio`, `HIIT`, `Mobility`)<br/>- Biomechanics Exercise Library with Target Muscle Groups, Equipment & Video Guides<br/>- **Live Workout Session Tracker** (`POST /api/v1/sessions/start` & `/complete`) | - Active-session conflict guard inside `IUnitOfWork` prevents an athlete from starting two concurrent `InProgress` workout sessions<br/>- Exposes `WorkoutSessionGrpcService.CompleteSession` so `ProgressTrackingService` can atomically verify and complete live sessions during workout logging |
| **Progress Analytics, Streaks & Gamification** | - Workout Completion Logging (Duration, Calories Burned, Difficulty, Rating, Exercise Sets/Reps/Weights)<br/>- Chronological Weight Logging with automatic **BMI** & **Total Weight Lost** derivation<br/>- **Daily Habit Streak Tracker** (`CurrentStreak`, `LongestStreak`)<br/>- **Gamified Milestone Achievements** (`TotalWorkouts`, `StreakDays`, `TotalCalories`, `WeightLostKg`)<br/>- **Period-Over-Period Analytics Dashboards** (`week`, `month`, `year`, `all`, `custom`) with percentage trend comparisons | - Pure domain engines: [`StreakCalculator`](./ProgressTrackingService/Common/Domain/StreakCalculator.cs), [`BmiCalculator`](./ProgressTrackingService/Common/Domain/BmiCalculator.cs), [`PeriodResolver`](./ProgressTrackingService/Common/Domain/PeriodResolver.cs), and [`AchievementRuleEvaluator`](./ProgressTrackingService/Common/Domain/AchievementRuleEvaluator.cs)<br/>- **Read-Time Streak Decay (`EffectiveCurrent`):** reports `0` when `LastWorkoutDate < Today - 1` without requiring costly midnight batch writes<br/>- **Cache-Aside + Post-Commit Redis Invalidation** (`pts:dashboard:*`, `pts:stats:*`) |
| **Subscription Billing, Paymob Checkout & Edge QoS** | - Subscription Status Inspection (`Free` vs. `Premium`, `Active`, `Cancelled`, `Expired`, `Refunded`)<br/>- Multi-Duration Premium Plans (`1`, `3`, `6`, or `12` months in EGP)<br/>- Native **Paymob 3DS Unified Checkout** (Cards, Meeza, Mobile Wallets)<br/>- Non-Destructive Cancellation (disables `AutoRenew` while preserving paid access until `ExpiresAtUtc`)<br/>- **Automatic Duplicate-Charge Refunds** & **Explicit Customer Refunds** | - **Idempotency-Key** protection on checkout intention creation<br/>- **20-Field Lexicographical HMAC-SHA512** webhook verification via `CryptographicOperations.FixedTimeEquals`<br/>- **Outbox-Driven Refund Saga** (`ProcessPaymobRefundConsumer` + `PendingRefundSweeper`)<br/>- **Dynamic Gateway Rate Limiting:** `100 req/hr` (Free) vs. `2,000 req/hr` (Premium) enforced at `ApiGateway` |

---

## 2. Business Workflows & Domain State Machines

### 2.1 End-to-End Athlete Journey (6-Stage Lifecycle Graph)

The diagram below traces how an athlete's actions flow across all 7 domain microservices and the API Gateway from day-one registration to daily training and premium subscription upgrades:

```mermaid
flowchart TD
    subgraph Stage1["Stage 1: Identity & Account Security (AuthenticationService)"]
        A1["POST /api/v1/auth/register<br/>Creates Identity + 6-Digit Email OTP"] --> A2["POST /api/v1/auth/verify-otp<br/>Confirms Email (Max 5 Attempts)"]
        A2 --> A3["POST /api/v1/auth/login<br/>Mints 15m RS256 JWT + 7d Refresh Token<br/>+ Parallel gRPC Onboarding/Premium Check"]
    end

    subgraph Stage2["Stage 2: Biometric Profile & Preferences (UserProfileService)"]
        B1["Consumes UserRegisteredEvent<br/>Provisions Default Profile & Settings"] --> B2["PUT /api/v1/profile & /settings<br/>Configures Biometrics, Units & Theme"]
    end

    subgraph Stage3["Stage 3: Clinical Metabolic Prescription (FitnessCalculationService)"]
        C1["POST /api/v1/fitness/stats & /calculate<br/>Computes Mifflin-St Jeor BMR & TDEE"] --> C2["Classifies Calorie Status (Weak / Normal / Hard)<br/>& Matches 1 of 15 Workout Plan Configs"]
        C2 --> C3["POST /api/v1/fitness/assign-plan<br/>Assigns Active Plan & Publishes Events"]
    end

    subgraph Stage4["Stage 4: Daily Nutrition & Live Training (NutritionService & WorkoutService)"]
        D1["GET /api/v1/nutrition/recommendations<br/>Fetches Calorie Target via gRPC from FCE"] --> D2["Returns Macro-Aligned Meals Filtered by<br/>MealType, Calorie Budget & Min Protein"]
        D3["POST /api/v1/sessions/start<br/>Verifies No Conflict & Starts Live Session"] --> D4["Athlete Performs Prescribed Exercises<br/>(Sets, Reps, Rest Intervals)"]
    end

    subgraph Stage5["Stage 5: Progress Logging, Streaks & Trophies (ProgressTrackingService)"]
        E1["POST /api/v1/progress/workouts<br/>Completes Session via gRPC to WorkoutService"] --> E2["Updates Consecutive Daily Streak<br/>& Evaluates Milestone Achievements"]
        E3["POST /api/v1/progress/weight<br/>Logs New Weight & Derives BMI"] --> E4["Publishes WeightUpdatedEvent<br/>Triggers Automatic FCE Recalculation"]
        E2 & E3 --> E5["GET /api/v1/progress?period=week<br/>Serves Redis-Cached Analytics Dashboard"]
    end

    subgraph Stage6["Stage 6: Monetization & Edge Tier Unlocking (SubscriptionBillingService + ApiGateway)"]
        F1["POST /api/v1/subscription/upgrade<br/>Creates Paymob 3DS Checkout Intention"] --> F2["POST /api/v1/subscription/webhooks/paymob<br/>Validates 20-Field HMAC-SHA512 Signature"]
        F2 --> F3["Activates Premium Tier, Caches tier:{userId} in Redis,<br/>& Publishes SubscriptionUpgradedEvent"]
        F3 --> F4["ApiGateway Immediately Unlocks<br/>2,000 req/hr Premium Rate Quota"]
    end

    A1 -.->|"RabbitMQ Outbox"| B1
    A3 --> B2
    B2 --> C1
    C3 -.->|"gRPC (Protobuf)"| D1
    C3 --> D3
    D4 --> E1
    E4 -.->|"RabbitMQ Outbox"| C1
    F3 -.->|"RabbitMQ Outbox"| B2
```

---

### 2.2 Clinical Metabolic Engine & 15-Plan Prescription Decision Graph

`FitnessCalculationService` uses a deterministic mathematical pipeline to convert raw biometrics into daily caloric targets and automatically prescribe or adapt the athlete's training program:

```mermaid
flowchart LR
    subgraph Inputs["1. Athlete Biometric Inputs"]
        Bio["Weight (40-200 kg)<br/>Height (140-220 cm)<br/>Age (16-100 yrs)<br/>Gender (Male / Female)"]
        Act["Activity Level:<br/>- Rookie (x1.20)<br/>- Beginner (x1.375)<br/>- Intermediate (x1.55)<br/>- Advance (x1.725)<br/>- TrueBeast (x1.90)"]
        Goal["Fitness Goal:<br/>- LoseWeight (-500 kcal)<br/>- GetFitter (0 kcal)<br/>- GainWeight (+300 kcal)<br/>- GainMoreFlexible (+150 kcal)<br/>- LearnTheBasic (0 kcal)"]
    end

    subgraph MathEngine["2. MetabolicCalculator (Mifflin-St Jeor)"]
        BMR["BMR Calculation:<br/>Male: 10W + 6.25H - 5A + 5<br/>Female: 10W + 6.25H - 5A - 161"]
        TDEE["TDEE Calculation:<br/>TDEE = BMR * ActivityFactor"]
        Target["Daily Calorie Target:<br/>Target = TDEE + GoalOffset"]
    end

    subgraph TierClassifier["3. Calorie Status Tier"]
        Weak["Weak Tier<br/>(Target <= 1800.00 kcal)"]
        Normal["Normal Tier<br/>(1800.01 - 2500.00 kcal)"]
        Hard["Hard Tier<br/>(Target > 2500.00 kcal)"]
    end

    subgraph PlanMatrix["4. Automated 15-Plan Prescription Matrix (Goal x Status)"]
        P_LW["Lose Weight Plans:<br/>LW-W (Fat Loss Kickstart)<br/>LW-N (Shred & Tone)<br/>LW-H (Extreme Cut)"]
        P_GF["Get Fitter Plans:<br/>GF-W (Foundation Fitness)<br/>GF-N (Total Body Sculpt)<br/>GF-H (Beast Conditioning)"]
        P_GW["Gain Weight Plans:<br/>GW-W (Gentle Surplus)<br/>GW-N (Mass Builder)<br/>GW-H (Maximum Bulk)"]
        P_GMF["Flexibility Plans:<br/>GMF-W (Gentle Mobility)<br/>GMF-N (Dynamic Flexibility)<br/>GMF-H (Athletic Mobility)"]
        P_LTB["Basics Plans:<br/>LTB-W (Intro to Movement)<br/>LTB-N (Fundamentals First)<br/>LTB-H (Fast-Track Athlete)"]
    end

    Bio --> BMR
    BMR & Act --> TDEE
    TDEE & Goal --> Target
    Target --> Weak & Normal & Hard
    Weak & Normal & Hard --> P_LW & P_GF & P_GW & P_GMF & P_LTB
```

---

### 2.3 Live Workout Session, Habit Streak & Gamification State Machine

When an athlete starts and completes a workout, `WorkoutService` and `ProgressTrackingService` coordinate across state boundaries to enforce session integrity, compute UTC calendar streaks, handle read-time streak decay, and award milestone achievements:

```mermaid
stateDiagram-v2
    [*] --> NoActiveSession

    state "WorkoutService (Session State)" as WS {
        NoActiveSession --> InProgress : POST /api/v1/sessions/start\n(CheckActiveSessionQuery == false)
        InProgress --> InProgress : Duplicate Start Blocked\n(409 Conflict)
        InProgress --> Completed : POST /api/v1/progress/workouts\n(Triggers gRPC CompleteSession)
    }

    state "ProgressTrackingService (Streak & Achievement State)" as PTS {
        Completed --> StreakEvaluation : Atomic UnitOfWork Transaction

        state StreakEvaluation {
            [*] --> FirstWorkoutEver : LastWorkoutDate == null\n(CurrentStreak = 1, Longest = 1)
            [*] --> SameCalendarDay : CompletedDate == LastWorkoutDate\n(Streak Preserved, Updated = false)
            [*] --> ConsecutiveDay : CompletedDate == LastWorkoutDate + 1d\n(CurrentStreak++, Update Longest)
            [*] --> BrokenStreakGap : CompletedDate >= LastWorkoutDate + 2d\n(CurrentStreak Reset to 1)
            [*] --> HistoricalBackdate : CompletedDate < LastWorkoutDate\n(Logged Without Altering Streak)
        }

        StreakEvaluation --> AchievementEvaluator : Evaluate Rules:\n- TotalWorkouts\n- StreakDays\n- TotalCalories\n- WeightLostKg
        AchievementEvaluator --> PostCommitCacheEviction : Commit SQL + Outbox\n& Evict pts:* Redis Keys
    }

    PostCommitCacheEviction --> ReadTimeDecayCheck : GET /api/v1/progress/stats/{userId}
    ReadTimeDecayCheck --> ActiveStreakDisplayed : LastWorkoutDate >= Today - 1d\n(Returns CurrentStreak)
    ReadTimeDecayCheck --> DecayedToZero : LastWorkoutDate < Today - 1d\n(Projects EffectiveCurrent = 0 Without DB Write)
```

---

### 2.4 Subscription Billing, Renewal & Auto-Refund Lifecycle State Machine

`SubscriptionBillingService` models every subscription and payment attempt through a deterministic fintech state machine with optimistic concurrency (`RowVersion`), idempotency keys, background sweepers, and automatic duplicate-charge refunds:

```mermaid
stateDiagram-v2
    [*] --> FreeTier : Default User State\n(100 req/hr Gateway Quota)

    FreeTier --> IntentionCreated : POST /api/v1/subscription/upgrade\n(Paymob Intention API -> PendingRedirect)
    IntentionCreated --> PaymentFailed : Webhook success == false\n(Logs FailureReason)
    PaymentFailed --> IntentionCreated : User Retries Checkout

    IntentionCreated --> ActivePremium : Webhook success == true\n(HMAC-SHA512 Verified)\nCaches tier:{userId} = Premium (2,000 req/hr)

    state "Race Condition / Duplicate Charge Protection" as RaceGuard {
        ActivePremium --> DuplicateChargeDetected : Second Successful Webhook Arrives\n(Different PaymobTransactionId while Active)
        DuplicateChargeDetected --> AutoRefundPending : Atomic SQL Commit:\nInsert RefundRequest + Outbox Command
        AutoRefundPending --> AutoRefunded : ProcessPaymobRefundConsumer Calls\nPaymob Void/Refund API (Subscription Stays Active)
    }

    ActivePremium --> CancelledPendingExpiry : POST /api/v1/subscription/cancel\n(AutoRenew = false, Access Kept Until ExpiresAtUtc)
    ActivePremium --> Expired : SubscriptionExpirationSweeper\n(ExpiresAtUtc <= UtcNow)\nEvicts Redis Tier & Emits SubscriptionExpiredEvent
    CancelledPendingExpiry --> Expired : SubscriptionExpirationSweeper\n(ExpiresAtUtc <= UtcNow)

    ActivePremium --> ExplicitlyRefunded : POST /api/v1/subscription/refund\nImmediately Revokes Access, Evicts Redis Tier,\n& Dispatches Outbox Refund to Paymob

    Expired --> FreeTier : Reverts to 100 req/hr Quota
    ExplicitlyRefunded --> FreeTier : Reverts to 100 req/hr Quota
```

---

## 3. Technical & Distributed Systems Architecture Graphs

### 3.1 High-Level Cloud-Native System Topology

All external traffic enters through **`ApiGateway` (Port `5000`)**, which enforces edge security and rate limiting before routing requests to the 7 isolated microservices:

```mermaid
flowchart TB
    Client(["Mobile / Web Client"])

    subgraph Edge["Edge Security & Routing Layer"]
        GW["ApiGateway (Ocelot :5000)<br/>- RS256 Public Key Validation (0 ms)<br/>- TokenRevocationMiddleware (Redis MGET ~0.3 ms)<br/>- DynamicTierRateLimiting (100 vs 2,000 req/hr)<br/>- Polly QoS Circuit Breakers & Correlation ID"]
    end

    subgraph Services["Domain Microservices (.NET 9 / Vertical Slice / CQRS)"]
        Auth["AuthenticationService (:8089)<br/>RS256 Minting, OTP, Refresh Rotation,<br/>RFC 6819 Theft Detection"]
        Profile["UserProfileService (:8080)<br/>Biometrics, Fitness Goals,<br/>Settings & Preferences"]
        FCE["FitnessCalculationService (:8081)<br/>Mifflin-St Jeor BMR/TDEE,<br/>15-Plan Prescription Engine"]
        Nutrition["NutritionService (:8085)<br/>Meal Plans, Allergens,<br/>gRPC Macro Recommendations"]
        Workout["WorkoutService (:8091)<br/>Exercises, Workout Plans,<br/>Live Session State Machine"]
        Progress["ProgressTrackingService (:8090)<br/>Weight/Workout Logs, Streaks,<br/>Achievements, Dashboards"]
        Billing["SubscriptionBillingService (:8092)<br/>Paymob Checkout, HMAC-SHA512 Webhooks,<br/>Atomic Auto-Refunds, Sweepers"]
    end

    subgraph DataLayer["Distributed Data, Caching & Messaging Infrastructure"]
        Redis[("Redis 7 (:6379)<br/>- JTI Denylist & User Cutoffs<br/>- Subscription Tier & Rate Counters<br/>- FCE Metrics & Plan Cache<br/>- Progress Dashboard Cache")]
        RMQ[["RabbitMQ 3 (:5672)<br/>MassTransit Topic Exchanges<br/>+ EF Core Transactional Outbox"]]
        SQL[("SQL Server 2022 (:1433)<br/>7 Isolated Databases:<br/>- AuthenticationDb<br/>- UserProfileDb<br/>- FitnessCalculationDb<br/>- NutritionDb<br/>- WorkoutDb<br/>- ProgressTrackingDb<br/>- SubscriptionBillingDb")]
    end

    Paymob(["Paymob Accept Cloud<br/>Unified Intention & Refund APIs"])

    Client ==>|"HTTPS / Bearer JWT"| GW
    GW --> Auth & Profile & FCE & Nutrition & Workout & Progress & Billing

    GW <-->|"MGET Revocation & Rate Limits"| Redis
    Auth & FCE & Progress & Billing <-->|"Cache & Security State"| Redis

    Auth & Profile & FCE & Progress & Billing <-->|"Publish / Consume via Outbox"| RMQ
    Auth & Profile & FCE & Nutrition & Workout & Progress & Billing -->|"EF Core 9"| SQL
    Billing <-->|"REST + HMAC-SHA512 Webhook"| Paymob
```

---

### 3.2 API Gateway Edge Security & Dynamic Rate-Limiting Pipeline

Every HTTP request arriving at `ApiGateway` passes through a 6-stage deterministic middleware pipeline before reaching any downstream container:

```mermaid
flowchart LR
    Req(["Incoming HTTP Request"]) --> M1["1. Correlation ID<br/>Injects / Propagates<br/>X-Correlation-ID"]
    M1 --> M2["2. CORS Policy<br/>GatewayCorsPolicy<br/>Validation"]
    M2 --> M3["3. RS256 JwtBearer<br/>Verifies Signature, exp,<br/>iss, aud via Public Key<br/>(0 ms CPU-only)"]
    M3 --> M4{"4. TokenRevocationMiddleware<br/>Single Redis MGET (~0.3 ms)<br/>- auth:revoked:jti:{jti}<br/>- auth:revoked-before:{sub}"}

    M4 -->|"JTI Blacklisted OR<br/>iat <= userCutoff"| R401["401 Unauthorized<br/>AUTH_TOKEN_REVOKED"]
    M4 -->|"Token Clean"| M5{"5. DynamicTierRateLimiting<br/>Reads tier:{sub} & Increments<br/>rate:user:{sub}:{yyyyMMddHH}"}

    M5 -->|"Free > 100 req/hr OR<br/>Premium > 2,000 req/hr"| R429["429 Too Many Requests<br/>RATE_LIMIT_EXCEEDED"]
    M5 -->|"Within Quota"| M6["6. Ocelot + Polly QoS<br/>Circuit Breaker &<br/>Downstream Forwarding"]
```

---

### 3.3 Internal Microservice Architecture (Vertical Slice + CQRS + Outbox)

Instead of traditional N-tier horizontal layers (`Controllers -> MegaServices -> Repositories`), every microservice in NextRep is structured around **Vertical Feature Slices** with **MediatR Orchestrators**, **Pure Domain Calculators**, and **Unit of Work Transaction Savepoints**:

```mermaid
flowchart TB
    subgraph Slice["Vertical Feature Slice (e.g., Features/Workouts/LogWorkout)"]
        EP["1. Minimal API Endpoint / Controller<br/>Extracts Claims, Maps Request -> Sends to IMediator"]
        Val["2. MediatR ValidationBehavior<br/>Executes All FluentValidation Rules In-Memory"]
        Orch["3. Feature Orchestrator Handler<br/>Coordinates Multi-Step Workflow"]
    end

    subgraph ExternalSync["External Synchronous I/O (Outside SQL Transaction)"]
        gRPCOut["Outbound gRPC / HTTP Client<br/>(Protected by Polly Retry + Circuit Breaker)<br/>Executed BEFORE or AFTER SQL Transaction"]
    end

    subgraph AtomicBoundary["4. Atomic SQL Transaction Boundary (IUnitOfWork.ExecuteAsync)"]
        SubQ["CQRS Read Queries<br/>(.AsNoTracking() Projections)"]
        PureDom["Pure Stateless Domain Engine<br/>(MetabolicCalculator / StreakCalculator /<br/>AchievementRuleEvaluator / HmacValidator)"]
        SubCmd["CQRS Write Commands<br/>(Mutate Aggregate Tables)"]
        OutboxStage["MassTransit Bus Outbox<br/>(Stages Integration Event into OutboxMessage Table)"]
        Commit["Single Atomic SQL Commit<br/>(Domain Rows + InboxState + OutboxMessage)"]
    end

    subgraph PostCommit["5. Post-Commit Side Effects"]
        CacheEvict["Redis Cache Invalidation<br/>(Evicts Stale Keys Only After SQL Commit Succeeds)"]
        OutboxWorker["MassTransit Outbox Background Dispatcher<br/>(Polls OutboxMessage & Publishes to RabbitMQ)"]
    end

    EP --> Val --> Orch
    Orch <-->|"No DB Locks Held"| gRPCOut
    Orch --> SubQ --> PureDom --> SubCmd --> OutboxStage --> Commit
    Commit --> CacheEvict
    Commit -.-> OutboxWorker
```

---

### 3.4 Distributed Redis Keyspace & Multi-Layer Caching Topology

Redis 7 (`StackExchange.Redis`) serves four distinct architectural roles across NextRep, partitioned by key prefix and TTL strategy:

```mermaid
flowchart TD
    RedisRoot[("Shared Redis 7 Instance (:6379)")]

    RedisRoot --> NS1["1. Security & Token Revocation Namespace (auth:*)"]
    RedisRoot --> NS2["2. Edge Tier & Rate Limiting Namespace (tier:* / rate:*)"]
    RedisRoot --> NS3["3. Metabolic Engine Cache Namespace (fce:*)"]
    RedisRoot --> NS4["4. Progress & Analytics Cache Namespace (pts:*)"]

    NS1 --> K1["auth:revoked:jti:{jti}<br/>TTL: Remaining JWT Lifetime (<= 15m)<br/>Purpose: Single-device logout denylist"]
    NS1 --> K2["auth:revoked-before:{userId}<br/>TTL: 15 Minutes (900s)<br/>Purpose: Multi-device cutoff & theft lockdown"]
    NS1 --> K3["auth:otp-attempts:{email}<br/>TTL: 10 Minutes<br/>Purpose: Max 5 failed OTP brute-force counter"]
    NS1 --> K4["auth:verified-otp:{email}:{otp}<br/>TTL: 10 Minutes<br/>Purpose: 2-step password reset verification marker"]

    NS2 --> K5["tier:{userId}<br/>TTL: Matches Subscription Expiry<br/>Purpose: Instant Free vs Premium lookup at Gateway"]
    NS2 --> K6["rate:user:{userId}:{yyyyMMddHH}<br/>TTL: 1 Hour Sliding Window<br/>Purpose: 100 vs 2,000 req/hr quota counter"]

    NS3 --> K7["fce:metrics:user:{userId}<br/>TTL: 5 Minutes (Cache-Aside)<br/>Evicted post-commit on metric recalculation"]
    NS3 --> K8["fce:plancfg:goal:{g}:status:{s}<br/>TTL: 24 Hours (Cache-Aside)<br/>Caches 15 seeded workout plan templates"]

    NS4 --> K9["pts:stats:{userId} (TTL: 10m)<br/>pts:dashboard:{userId}:{period} (TTL: 5m)<br/>pts:achievements:{userId} (TTL: 30m)<br/>pts:weighthistory:{userId}:* (TTL: 5m)<br/>Evicted atomically post-commit via CacheInvalidator"]
```

---

## 4. Inter-Service Communication Graphs (RabbitMQ & gRPC)

NextRep separates **asynchronous state propagation** (event-driven choreography via RabbitMQ) from **synchronous real-time queries/commands** (gRPC over HTTP/2).

### 4.1 Asynchronous Event-Driven Choreography (RabbitMQ + MassTransit Outbox/Inbox)

Every event below is persisted inside the producer's SQL Server `OutboxMessage` table within the same database transaction as the domain entity mutation, and deduplicated by the consumer's `InboxState` table:

```mermaid
flowchart LR
    subgraph Producers["Event Producers (EF Core Transactional Outbox)"]
        P_Auth["AuthenticationService"]
        P_Profile["UserProfileService"]
        P_FCE["FitnessCalculationService"]
        P_Progress["ProgressTrackingService"]
        P_Billing["SubscriptionBillingService"]
    end

    subgraph Broker["RabbitMQ Topic Exchanges"]
        E1(["UserRegisteredEvent"])
        E2(["ProfileCompletedEvent"])
        E3(["FitnessStatsSubmittedEvent"])
        E4(["FitnessMetricsRecalculatedEvent"])
        E5(["FitnessPlanAssignedEvent"])
        E6(["WeightUpdatedEvent"])
        E7(["WorkoutLoggedEvent"])
        E8(["AchievementEarnedEvent"])
        E9(["SubscriptionUpgradedEvent"])
        E10(["SubscriptionExpiredEvent"])
        E11(["PaymentRefundedEvent"])
        E12(["ExecutePaymobRefundCommandEvent<br/>(Internal Durable Queue)"])
    end

    subgraph Consumers["Idempotent Event Consumers (EF Core Transactional Inbox)"]
        C_Profile["UserProfileService<br/>- UserRegisteredConsumer<br/>- SubscriptionUpgradedConsumer<br/>- SubscriptionExpiredConsumer"]
        C_FCE["FitnessCalculationService<br/>- WeightUpdatedConsumer<br/>(Watermark Guard: RecordedAt > LastWeightEventAt)"]
        C_Progress["ProgressTrackingService<br/>- UserRegisteredConsumer (Seeds 0-Streak Baseline)<br/>- FitnessStatsSubmittedConsumer (Syncs HeightCm)"]
        C_Billing["SubscriptionBillingService<br/>- ProcessPaymobRefundConsumer"]
    end

    P_Auth -->|"On Signup"| E1
    E1 --> C_Profile & C_Progress

    P_Profile -->|"On Onboarding"| E2
    E2 --> C_FCE & C_Progress

    P_FCE -->|"On Stats / Plan Calc"| E3 & E4 & E5
    E3 --> C_Progress

    P_Progress -->|"On Latest Weight Check-In"| E6
    P_Progress -->|"On Workout / Trophy"| E7 & E8
    E6 --> C_FCE

    P_Billing -->|"On Webhook / Sweeper"| E9 & E10 & E11
    P_Billing -->|"On Duplicate Charge / Refund"| E12
    E9 & E10 --> C_Profile
    E12 --> C_Billing
```

#### Complete Integration Event Catalog

| Event / Message Contract | Producer | Consumer(s) | Business Purpose & Idempotency Mechanism |
| :--- | :--- | :--- | :--- |
| `UserRegisteredEvent` | `AuthenticationService` | `UserProfileService`, `ProgressTrackingService` | Provisions default `UserProfile` + `UserSettings` rows and initializes baseline `UserStatistics` / `Streaks` records (`0` workouts, `0` streak). |
| `ProfileCompletedEvent` | `UserProfileService` | `FitnessCalculationService`, `ProgressTrackingService` | Signals completion of biometric onboarding so downstream services can initialize metabolic targets and progress tracking. |
| `FitnessStatsSubmittedEvent` | `FitnessCalculationService` | `ProgressTrackingService` | Synchronizes the athlete's `HeightCm` into `UserBiometrics` so `ProgressTrackingService` can compute accurate BMI values on every weight check-in. |
| `FitnessMetricsRecalculatedEvent` | `FitnessCalculationService` | *Topic Subscribers* | Broadcasts updated BMR, TDEE, Daily Calorie Target, and Calorie Status Tier whenever metrics are recalculated. |
| `FitnessPlanAssignedEvent` | `FitnessCalculationService` | *Topic Subscribers* | Broadcasts newly assigned or adapted workout plan (`PlanId`, `Goal`, `Status`) when an athlete's calorie tier shifts. |
| `WeightUpdatedEvent` | `ProgressTrackingService` | `FitnessCalculationService` | Triggered when a chronologically newest weigh-in is logged; causes FCE to recalculate BMR/TDEE and adapt the active plan if the intensity tier changed. Protected by `LastWeightEventAt` watermark. |
| `WorkoutLoggedEvent` | `ProgressTrackingService` | *Topic Subscribers* | Emitted atomically when a workout session is logged and streaks are updated. |
| `AchievementEarnedEvent` | `ProgressTrackingService` | *Topic Subscribers* | Emitted whenever `AchievementRuleEvaluator` unlocks a new milestone trophy badge. |
| `SubscriptionUpgradedEvent` | `SubscriptionBillingService` | `UserProfileService` | Updates `IsPremiumCached = true` on `UserProfile` upon verified Paymob payment. |
| `SubscriptionExpiredEvent` | `SubscriptionBillingService` | `UserProfileService` | Updates `IsPremiumCached = false` when `SubscriptionExpirationSweeper` transitions an expired subscription. |
| `ExecutePaymobRefundCommandEvent` | `SubscriptionBillingService` | `SubscriptionBillingService` | Durable internal outbox command that executes Paymob HTTP refunds (`POST /api/acceptance/void_refund/refund`) with retry tracking and `PendingRefundSweeper` backup. |
| `PaymentRefundedEvent` | `SubscriptionBillingService` | *Topic Subscribers* | Audit notification emitted after `ProcessPaymobRefundConsumer` confirms a completed Paymob refund transaction. |

---

### 4.2 Synchronous High-Performance RPC Mesh (gRPC over HTTP/2 + Polly)

When a microservice requires immediate data or state verification from another bounded context during an active HTTP request, it uses **gRPC (Protocol Buffers over HTTP/2)** wrapped in **Polly resilience pipelines**:

```mermaid
flowchart LR
    Auth["AuthenticationService"]
    Nutrition["NutritionService"]
    Progress["ProgressTrackingService"]

    Profile["UserProfileService<br/>(GrpcServices/ProfileGrpcService.cs)"]
    Billing["SubscriptionBillingService<br/>(GrpcServices/SubscriptionGrpcService.cs)"]
    FCE["FitnessCalculationService<br/>(GrpcServices/CalorieTargetGrpcService.cs)"]
    Workout["WorkoutService<br/>(GrpcServices/WorkoutSessionGrpcServiceImpl.cs)"]

    Auth ==>|"1. ProfileGrpc.GetUserProfileStatus<br/>(HasCompletedProfile)"| Profile
    Auth ==>|"2. SubscriptionGrpc.CheckUserSubscription<br/>(IsPremiumUser)"| Billing
    Nutrition ==>|"3. CalorieTargetService.GetUserMetrics<br/>(CalorieTarget, IsCalculated)"| FCE
    Progress ==>|"4. WorkoutSessionGrpcService.CompleteSession<br/>(SessionId, UserId)"| Workout
```

| # | gRPC Client (Caller) | gRPC Server (Callee) | Proto Contract | Purpose & Resilience Strategy |
| :- | :--- | :--- | :--- | :--- |
| **1** | `AuthenticationService` | `UserProfileService` | `ProfileGrpc.GetUserProfileStatus` | Enriches the login response with `hasCompletedProfile` so the client app knows whether to route to Onboarding or Home. Runs **outside** the SQL transaction; falls back to `false` on timeout. |
| **2** | `AuthenticationService` | `SubscriptionBillingService` | `SubscriptionGrpc.CheckUserSubscription` | Enriches the login response with `isPremium`. Executed concurrently with #1 via `Task.WhenAll` outside the SQL lock. |
| **3** | `NutritionService` | `FitnessCalculationService` | `CalorieTargetService.GetUserMetrics` | Retrieves the user's calculated daily calorie target to recommend macro-aligned meals. Protected by Polly retry (`3 attempts`) + circuit breaker (`GrpcPolicies.cs`). |
| **4** | `ProgressTrackingService` | `WorkoutService` | `WorkoutSessionGrpcService.CompleteSession` | Synchronously verifies session ownership and transitions an `InProgress` `WorkoutSession` to `Completed` before recording workout logs, updating streaks, and awarding achievements. |

---

## 5. Hard Distributed Systems & Security Engineering Challenges Solved

### 5.1 RS256 Asymmetric JWTs & Sub-Millisecond (`< 0.3 ms`) Hybrid Redis Token Revocation

#### The Architectural Problem
In a distributed microservices platform, verifying tokens by calling `AuthenticationService` on every HTTP request creates a **Single Point of Failure (SPOF)** and adds `10–25 ms` of network/SQL latency to every API call.
To avoid this, NextRep uses **RS256 (RSA Signature with SHA-256)**:
- Only `AuthenticationService` holds the **2048-bit RSA Private Key** (`auth-service-private.pem`) to mint tokens.
- `ApiGateway` and all downstream microservices hold the **read-only RSA Public Key** (`auth-service-public.pem`) to verify token signatures **100% locally in CPU memory (`0 ms`)**.

**However, pure stateless JWTs have a critical security flaw:** if a user logs out, changes their password after a breach, or has their token stolen, a traditional stateless JWT **remains completely valid across every microservice until its `exp` timestamp expires.**

#### How We Solved It (Hybrid #1 + #2 + #3 Pipeline)
We engineered a **3-layer hybrid revocation architecture** shared between `AuthenticationService` and `ApiGateway` via Redis:

1. **Short-Lived Access Token TTL (`15 Minutes`):** Bounds the maximum lifetime of any Redis denylist key to at most 900 seconds, keeping Redis memory consumption under **`1 MB` even at 10,000 concurrent logouts**.
2. **Per-Token `jti` Denylist (`auth:revoked:jti:{jti}`):** When a user calls `POST /api/v1/auth/logout`, [`LogoutOrchestratorHandler`](./AuthenticationService/Features/Auth/Orchestrators/Logout/LogoutOrchestratorHandler.cs) extracts the token's unique `jti` (`JwtRegisteredClaimNames.Jti`) and remaining lifetime (`exp - UtcNow`), writing `SET auth:revoked:jti:{jti} "1" EX <remainingSeconds>`. This revokes that specific device immediately without kicking the user off their other devices.
3. **Per-User Timestamp Cutoff (`auth:revoked-before:{userId}`):** When a user calls `POST /api/v1/auth/logout-all`, changes their password, resets their password, or triggers refresh-token theft detection, [`RedisTokenRevocationStore`](./AuthenticationService/Services/RedisTokenRevocationStore.cs) writes `SET auth:revoked-before:{userId} "<nowUnixSeconds>" EX 900`. Any JWT for that user whose `iat` (*Issued At*) claim is `<= cutoffUnix` is rejected across the entire platform in $O(1)$ time.
4. **Single-Roundtrip Atomic `MGET` at `ApiGateway`:** On every authenticated request, [`TokenRevocationMiddleware`](./ApiGateway/Middleware/TokenRevocationMiddleware.cs) batches both checks into a **single Redis `MGET` network packet (`~0.3 ms`)**:

```mermaid
sequenceDiagram
    autonumber
    actor Client as Mobile / Web Client
    participant GW as ApiGateway (TokenRevocationMiddleware)
    participant Redis as Shared Redis (:6379)
    participant Auth as AuthenticationService
    participant Sub as Downstream Microservice

    rect rgb(235, 248, 240)
    Note over Client,Sub: 1. Normal Authenticated Request (~0.3 ms overhead)
    Client->>GW: GET /api/v1/progress/dashboard<br/>Authorization: Bearer <JWT (jti=abc, sub=u1, iat=1762448000)>
    GW->>GW: Verify RS256 Signature & exp locally (0 ms)
    GW->>Redis: Single-Roundtrip MGET:<br/>["auth:revoked:jti:abc", "auth:revoked-before:u1"]
    Redis-->>GW: [nil, nil] (~0.3 ms)
    GW->>Sub: Forward authenticated request
    Sub-->>Client: 200 OK
    end

    rect rgb(255, 245, 235)
    Note over Client,Sub: 2. Single-Device Logout (POST /api/v1/auth/logout)
    Client->>GW: POST /api/v1/auth/logout (Bearer JWT + RefreshToken)
    GW->>Auth: Forward request
    Auth->>Auth: Verify RefreshToken belongs to sub=u1 & revoke in SQL Server
    Auth->>Redis: SET auth:revoked:jti:abc "1" EX (exp - now)
    Auth-->>Client: 200 OK ("Logged out successfully.")
    end

    rect rgb(255, 235, 235)
    Note over Client,Sub: 3. Stolen / Logged-Out Token Replay (Blocked in < 0.5 ms)
    Client->>GW: POST /api/v1/subscription/cancel<br/>Authorization: Bearer <Logged-Out JWT (jti=abc)>
    GW->>Redis: MGET ["auth:revoked:jti:abc", "auth:revoked-before:u1"]
    Redis-->>GW: ["1", nil] -> JTI IS REVOKED
    GW-->>Client: 401 Unauthorized (AUTH_TOKEN_REVOKED)
    Note over Sub: Downstream microservice is NEVER called
    end
```

---

### 5.2 RFC 6819 Refresh Token Reuse / Theft Detection & Multi-Device Session Cutoffs

#### The Problem
If an attacker steals a user's 7-day Refresh Token and rotates it first, the old Refresh Token is marked `RevokedAt != null` in SQL Server. When the legitimate user's phone later attempts to refresh using that original token, naive implementations simply return `401 Unauthorized` to the victim while **leaving the attacker's newly rotated Refresh Token active for 7 days.**

#### Our Solution
In [`RefreshTokenOrchestratorHandler.cs`](./AuthenticationService/Features/Auth/Orchestrators/RefreshToken/RefreshTokenOrchestratorHandler.cs), if an already-revoked Refresh Token (`RevokedAt != null`) is ever presented:
1. The server immediately recognizes a **Refresh Token Replay / Theft Event (RFC 6819 Section 5.2.2.3)**.
2. It executes [`RevokeAllRefreshTokensCommand`](./AuthenticationService/Features/Auth/Commands/RevokeAllRefreshTokens/RevokeAllRefreshTokensCommandHandler.cs) to invalidate **every active Refresh Token** for that user in SQL Server.
3. It writes `auth:revoked-before:{userId} = UtcNow` to Redis, immediately invalidating the attacker's active 15-minute JWT at `ApiGateway`.
4. It returns `401 Unauthorized` with error code `AUTH_TOKEN_THEFT_DETECTED`, forcing a clean re-authentication.

---

### 5.3 Native Paymob Integration, Constant-Time HMAC-SHA512 & Atomic Auto-Refunds on Race Conditions

#### The Problem
In payment systems, a user might accidentally pay twice (e.g., opening two checkout tabs or retrying a slow mobile connection), or a payment gateway might deliver two concurrent `TRANSACTION` webhooks for separate payment intentions. If the user's subscription was already activated by Transaction #1, simply ignoring Transaction #2 leaves the customer **double-charged in real money** with no extra subscription value.

#### Our Solution
In [`ProcessPaymobWebhookOrchestrator.cs`](./SubscriptionBillingService/Features/Subscriptions/Orchestrators/ProcessPaymobWebhookOrchestrator.cs):
1. **Cryptographic Webhook Authenticity:** [`PaymobHmacValidator`](./SubscriptionBillingService/Infrastructure/Paymob/PaymobHmacValidator.cs) extracts the 20 required Paymob transaction fields in strict lexicographical order, computes the `HMAC-SHA512` digest, and compares bytes using **`CryptographicOperations.FixedTimeEquals`** to prevent timing side-channel attacks.
2. **Idempotency vs. Duplicate-Charge Detection:**
   - If the *same* `ProviderTransactionId` arrives twice (webhook retry), it returns `200 OK` idempotently.
   - If a *different* successful `ProviderTransactionId` arrives for a user whose subscription is **already `Active` with `ExpiresAtUtc > UtcNow`**, the orchestrator detects a **Duplicate Charge Race Condition**:
     - Inside the **same `IUnitOfWork` SQL transaction**, it marks the `PaymentAttempt` as `DuplicateChargePendingRefund`, creates a `RefundRequest` (`Reason = DuplicateActiveSubscription`, `AmountCents`), writes an audit `BillingLog`, and stages `ExecutePaymobRefundCommandEvent` in the MassTransit Outbox.
     - [`ProcessPaymobRefundConsumer`](./SubscriptionBillingService/Features/Subscriptions/Consumers/ProcessPaymobRefundConsumer.cs) + [`PendingRefundSweeper`](./SubscriptionBillingService/Features/Subscriptions/Sweepers/PendingRefundSweeper.cs) automatically refund the user's card via Paymob's `/api/acceptance/void_refund/refund` endpoint.

```mermaid
sequenceDiagram
    autonumber
    participant Paymob as Paymob Cloud
    participant Sub as SubscriptionBillingService
    participant SQL as SubscriptionBillingDb (SQL + Outbox)
    participant RMQ as RabbitMQ (Outbox Consumer)

    Paymob->>Sub: POST /api/v1/subscription/webhooks/paymob?hmac=...<br/>(Tx #88776655, Success=true, 199.00 EGP)
    Sub->>Sub: Verify 20-Field HMAC-SHA512 (FixedTimeEquals)
    Sub->>SQL: Begin Transaction (IUnitOfWork)
    SQL-->>Sub: UserSubscription is ALREADY Active (from Tx #88776600)
    Note over Sub,SQL: Duplicate Charge Detected - Trigger Atomic Auto-Refund
    Sub->>SQL: 1. Update PaymentAttempt (Status=DuplicateChargePendingRefund)<br/>2. Insert RefundRequest (Status=Pending, AmountCents=19900)<br/>3. Insert BillingLog (Status=DuplicateChargeDetected)<br/>4. Insert OutboxMessage (ExecutePaymobRefundCommandEvent)
    Sub->>SQL: Commit Transaction Atomically
    Sub-->>Paymob: 200 OK (Webhook Acknowledged)

    SQL->>RMQ: Outbox Dispatches ExecutePaymobRefundCommandEvent
    RMQ->>Sub: ProcessPaymobRefundConsumer.Consume()
    Sub->>Paymob: POST /api/acceptance/void_refund/refund (Tx #88776655, 19900 cents)
    Paymob-->>Sub: 200 OK (RefundTxId = 99887766)
    Sub->>SQL: Update RefundRequest (Status=Refunded) & PaymentAttempt (Status=AutoRefunded)
```

---

### 5.4 Eliminating Dual-Write Bugs via the MassTransit EF Core Transactional Outbox

A classic microservices anti-pattern is calling `await _dbContext.SaveChangesAsync()` followed by `await _publishEndpoint.Publish(event)`. If the RabbitMQ broker is momentarily unreachable or the container crashes between those two lines, the database is updated but downstream services never receive the event (**inconsistent zombie state**).

```mermaid
flowchart LR
    subgraph AntiPattern["Dual-Write Anti-Pattern (Unsafe)"]
        A1["1. SaveChangesAsync() -> SQL Committed"] --> A2["2. Network Blip / Crash Before Publish()"]
        A2 --> A3["Result: DB Updated, Event Lost Forever"]
    end

    subgraph NextRepOutbox["NextRep Transactional Outbox Pattern (Atomic)"]
        B1["1. IUnitOfWork.ExecuteAsync() Begins SQL Transaction"] --> B2["2. Mutate Domain Tables + Stage OutboxMessage Row"]
        B2 --> B3["3. Single Atomic SQL Commit (Both Succeed or Both Roll Back)"]
        B3 --> B4["4. Background Outbox Worker Publishes to RabbitMQ<br/>& Deduplicates via Consumer InboxState Table"]
    end
```

---

### 5.5 Dynamic Subscription-Tier Rate Limiting at the Edge (`ApiGateway`)

Rather than applying a static rate limit to all users, [`DynamicTierRateLimitingMiddleware`](./ApiGateway/Middleware/DynamicTierRateLimitingMiddleware.cs) in `ApiGateway` inspects the user's live subscription tier in Redis (`tier:{userId}`) and enforces hourly sliding-window quotas (`rate:user:{userId}:{yyyyMMddHH}`):
- **Free Tier:** `100 requests / hour`
- **Premium Tier:** `2,000 requests / hour` (unlocked immediately when `SubscriptionBillingService` caches `tier:{userId} = "Premium"` upon payment)
- Every response includes `X-RateLimit-Limit`, `X-RateLimit-Remaining`, and `X-RateLimit-Tier` headers.

---

### 5.6 Decoupling External gRPC Network I/O from SQL Server Transactions

During our security and performance audit of [`LoginOrchestratorHandler.cs`](./AuthenticationService/Features/Auth/Orchestrators/Login/LoginOrchestratorHandler.cs), we identified that calling `UserProfileService` and `SubscriptionBillingService` via gRPC *inside* `_unitOfWork.ExecuteAsync` held SQL Server row locks open on `Users`, `LoginAttempts`, and `RefreshTokens` for the duration of external network I/O.
We refactored `LoginOrchestratorHandler` so the SQL transaction commits and releases its connection back to the pool **first**, and then executes both read-only gRPC enrichment calls concurrently via `Task.WhenAll` outside the transaction boundary.

---

## 6. Database-per-Service SQL Server Architecture (Complete ER Diagrams)

NextRep enforces strict **Database-per-Service** encapsulation on SQL Server 2022. Cross-service relationships are linked logically by `UserId (Guid)` and synchronized via integration events or gRPC.

```mermaid
erDiagram
    %% 1. AuthenticationDb
    AUTH_Users ||--o{ AUTH_RefreshTokens : "has sessions"
    AUTH_Users {
        string Id PK "GUID"
        string Email UK
        string PasswordHash
        bool EmailConfirmed
        bool IsLockedOut
        datetime LockedUntil
    }
    AUTH_RefreshTokens {
        int Id PK
        string UserId FK
        string Token
        datetime ExpiresAt
        datetime CreatedAt
        datetime RevokedAt "Nullable (RFC 6819 Theft Check)"
    }
    AUTH_OtpCodes {
        int Id PK
        string Email
        string Code "6-Digit OTP"
        bool IsUsed
        datetime ExpiresAt
    }
    AUTH_LoginAttempts {
        int Id PK
        string Email
        string IpAddress "X-Forwarded-For"
        bool IsSuccess
        datetime AttemptedAt
    }

    %% 2. UserProfileDb
    PROFILE_UserProfiles ||--o| PROFILE_UserSettings : "configures"
    PROFILE_UserProfiles {
        int Id PK
        string FirstName
        string LastName
        string Email UK
        string PhoneNumber
        string ProfilePictureUrl
        bool IsPremiumCached
        datetime MemberSince
    }
    PROFILE_UserSettings {
        int Id PK,FK
        string Language "en | ar"
        string Theme "light | dark"
        string NotificationSettings "JSON"
        string PrivacySettings "JSON"
        string UnitSettings "JSON"
    }

    %% 3. FitnessCalculationDb
    FCE_UserFitnessStats ||--o{ FCE_CalculatedMetrics : "source snapshot"
    FCE_CalculatedMetrics ||--o{ FCE_UserAssignedPlan : "determines status"
    FCE_FitnessPlanConfig ||--o{ FCE_UserAssignedPlan : "prescribes"
    FCE_UserAssignedPlan ||--o{ FCE_UserPlanHistory : "audited by"
    FCE_UserFitnessStats {
        uuid Id PK
        uuid UserId
        float Weight "40-200 kg"
        float Height "140-220 cm"
        int Age "16-100 yrs"
        int Gender "Male | Female"
        int Goal "0-4"
        int ActivityLevel "0-4"
        datetime RecordedAt
    }
    FCE_CalculatedMetrics {
        uuid Id PK
        uuid UserId UK
        float Bmr "Mifflin-St Jeor"
        float Tdee
        float CalorieTarget
        int Status "Weak | Normal | Hard"
        datetime LastWeightEventAt "Idempotency Watermark"
        binary RowVersion
    }
    FCE_FitnessPlanConfig {
        uuid Id PK
        string PlanId UK "LW-W .. LTB-H (15 Plans)"
        string Name
        int Goal
        int Status
        int WorkoutsPerWeek
        int EstimatedDurationWeeks
    }
    FCE_UserAssignedPlan {
        uuid Id PK
        uuid UserId
        uuid FitnessPlanConfigId FK
        bool IsActive
        datetime AssignedAt
    }
    FCE_UserPlanHistory {
        uuid Id PK
        uuid UserId
        string PlanId
        string PreviousPlanId
        string TriggeredBy
        datetime ChangedAt
    }

    %% 4. SubscriptionBillingDb
    SUB_PaymentAttempts ||--o{ SUB_BillingLogs : "has audit trail"
    SUB_PaymentAttempts ||--o| SUB_RefundRequests : "triggers refund"
    SUB_UserSubscriptions {
        guid Id PK
        guid UserId UK
        string Tier "Free | Premium"
        string Status "Active | Cancelled | Expired | Refunded"
        int PlanDurationMonths "1 | 3 | 6 | 12"
        bool AutoRenew
        datetime StartedAtUtc
        datetime ExpiresAtUtc
        byte[] RowVersion
    }
    SUB_PaymentAttempts {
        guid Id PK
        guid UserId
        string IdempotencyKey UK
        int AmountCents
        string Currency "EGP"
        string Status "Initiated | PendingRedirect | Succeeded | AutoRefunded"
        string ProviderIntentionId
        string ProviderTransactionId
        byte[] RowVersion
    }
    SUB_RefundRequests {
        guid Id PK
        guid PaymentAttemptId FK
        guid UserId
        string ProviderTransactionId UK
        string ProviderRefundTransactionId
        int AmountCents
        string Reason "DuplicateActiveSubscription | ExplicitSubscriptionRefund"
        string Status "Pending | Refunded | FailedManualIntervention"
        int RetryCount
    }
    SUB_BillingLogs {
        guid Id PK
        guid UserId
        guid PaymentAttemptId FK
        decimal Amount
        string ProviderTransactionId UK
        string PaymentStatus
        datetime CreatedAt
    }
```

```mermaid
erDiagram
    %% 5. NutritionDb
    NUT_MealPlans ||--o{ NUT_MealPlanItems : "schedules"
    NUT_Meals ||--o{ NUT_MealPlanItems : "assigned to"
    NUT_Meals ||--o{ NUT_MealIngredients : "contains"
    NUT_Ingredients ||--o{ NUT_MealIngredients : "measured in"
    NUT_Meals ||--o{ NUT_MealAllergens : "flags"
    NUT_Allergens ||--o{ NUT_MealAllergens : "warns"
    NUT_Meals ||--o{ NUT_MealTags : "categorized by"
    NUT_Tags ||--o{ NUT_MealTags : "labels"
    NUT_Meals {
        guid Id PK
        string Name
        string MealType "Breakfast | Lunch | Dinner | Snack"
        int Calories
        decimal ProteinGrams
        decimal CarbsGrams
        decimal FatsGrams
        decimal FiberGrams
        int PrepTimeMinutes
    }
    NUT_MealPlans {
        guid Id PK
        string Name
        int TargetCalories
        string GoalCategory
        int DurationDays
    }
    NUT_MealPlanItems {
        guid Id PK
        guid MealPlanId FK
        guid MealId FK
        int DayNumber
    }
    NUT_Ingredients {
        guid Id PK
        string Name
        string Category
    }
    NUT_MealIngredients {
        guid MealId PK,FK
        guid IngredientId PK,FK
        decimal Quantity
        string Unit
    }
    NUT_Allergens {
        guid Id PK
        string Name
        string Severity
    }
    NUT_MealAllergens {
        guid MealId PK,FK
        guid AllergenId PK,FK
    }
    NUT_Tags {
        guid Id PK
        string Name
    }
    NUT_MealTags {
        guid MealId PK,FK
        guid TagId PK,FK
    }

    %% 6. WorkoutDb
    WRK_WorkoutPlans ||--o{ WRK_Workouts : "includes"
    WRK_Workouts ||--o{ WRK_WorkoutExercises : "prescribes"
    WRK_Exercises ||--o{ WRK_WorkoutExercises : "performed in"
    WRK_Workouts ||--o{ WRK_WorkoutSessions : "tracked by"
    WRK_WorkoutPlans {
        guid Id PK
        string Name
        string DifficultyLevel
        int DurationWeeks
    }
    WRK_Workouts {
        guid Id PK
        guid WorkoutPlanId FK
        string Name
        string Category "Strength | Cardio | HIIT | Mobility"
        int EstimatedDurationMinutes
    }
    WRK_Exercises {
        guid Id PK
        string Name
        string TargetMuscleGroup
        string Equipment
    }
    WRK_WorkoutExercises {
        guid Id PK
        guid WorkoutId FK
        guid ExerciseId FK
        int Sets
        int Reps
        int RestSeconds
        int OrderIndex
    }
    WRK_WorkoutSessions {
        guid Id PK
        guid UserId
        guid WorkoutId FK
        string Status "InProgress | Completed"
        datetime StartedAt
        datetime CompletedAt
    }

    %% 7. ProgressTrackingDb
    PRG_WorkoutLogs ||--o{ PRG_WorkoutLogExercises : "contains"
    PRG_Achievements ||--o{ PRG_UserAchievements : "awarded as"
    PRG_WorkoutLogs {
        guid Id PK
        guid UserId
        guid WorkoutId
        string SessionId UK
        datetime CompletedAt
        int DurationInMinutes
        int CaloriesBurned
        string Difficulty
        int Rating
    }
    PRG_WorkoutLogExercises {
        guid Id PK
        guid WorkoutLogId FK
        guid ExerciseId
        int SetsCompleted
        int RepsCompleted
        double WeightUsed
    }
    PRG_WeightHistory {
        guid Id PK
        guid UserId
        double Weight
        datetime Date
    }
    PRG_UserStatistics {
        guid UserId PK
        int TotalWorkouts
        int TotalCaloriesBurned
        int TotalMinutes
        double InitialWeight
        double CurrentWeight
        double TotalWeightLost
    }
    PRG_Streaks {
        guid UserId PK
        int CurrentStreak
        int LongestStreak
        datetime LastWorkoutDate
        rowversion RowVersion
    }
    PRG_Achievements {
        guid Id PK
        string Code UK
        string Name
        string CriteriaType
        double Threshold
    }
    PRG_UserAchievements {
        guid Id PK
        guid UserId
        guid AchievementId FK
        datetime EarnedAt
    }
    PRG_UserBiometrics {
        guid UserId PK
        double HeightCm
    }
```

---

## 7. CI/CD Quality Gate & Container Delivery Pipeline

NextRep enforces an automated build, test, coverage, and container publishing pipeline in [`.github/workflows/production.yml`](./.github/workflows/production.yml). No Docker image can be built or pushed to Docker Hub unless all **225 automated tests** pass.

```mermaid
flowchart LR
    Push(["Git Push / PR to main"]) --> Tag["Stage 1: generate-tag<br/>Generates Semantic Version<br/>& Short SHA Tag"]
    Push --> Test["Stage 2: Automated Test Gate (.NET 9.0)<br/>- dotnet restore FitnessApp.slnx<br/>- dotnet build -c Release<br/>- dotnet test -c Release --collect:'XPlat Code Coverage'<br/>(225 Passed, 0 Failed across 5 Test Suites)"]
    Test --> Cov["Uploads TRX Test Report &<br/>Cobertura XML Coverage Artifacts"]
    Tag & Test --> Matrix["Stage 3: Parallel Matrix Docker Build & Push<br/>Builds Multi-Stage Alpine/Chiseled Images for All 8 Services<br/>& Pushes to Docker Hub Registry"]
```

---

## 8. Microservice Catalog & Service-Specific Documentation

Every service in the repository includes its own dedicated, deep-dive `README.md` with API endpoint tables, internal architecture diagrams, database configurations, and local execution instructions:

| Service | Host Port | Database | Core Responsibility | Documentation |
| :--- | :---: | :--- | :--- | :---: |
| **1. `ApiGateway`** | `:5000` | *Stateless (Redis)* | Ocelot Reverse Proxy, RS256 Edge Validation, `< 0.3 ms` Redis `MGET` Token Revocation, Dynamic Tier Rate Limiting (`100` vs `2,000 req/hr`), Polly QoS. | [Read Docs](./ApiGateway/README.md) |
| **2. `AuthenticationService`** | `:8089` | `AuthenticationDb` | RS256 JWT Issuance (`15m` TTL), Refresh Token Rotation & RFC 6819 Theft Detection, Redis `jti` Denylist & User Cutoff Store, 2-Step OTP Flow with Brute-Force Protection, Parallel gRPC Login Enrichment. | [Read Docs](./AuthenticationService/README.md) |
| **3. `UserProfileService`** | `:8080` | `UserProfileDb` | Biometric Onboarding, Fitness Goal Management, Dietary Preferences, Profile Completion State Machine, gRPC Profile Status Server, Event-Driven Weight & Subscription Sync. | [Read Docs](./UserProfileService/README.md) |
| **4. `FitnessCalculationService`** | `:8081` | `FitnessCalculationDb` | Mifflin-St Jeor BMR & TDEE Engine, Goal-Based Calorie & Macro Split Calculator, Automated 15-Plan Fitness Prescription Matrix, Redis L2 Cache, gRPC Calorie Target Server. | [Read Docs](./FitnessCalculationService/README.md) |
| **5. `NutritionService`** | `:8085` | `NutritionDb` | Curated Meal Plans, Ingredient & Allergen Graphs, Calorie-Filtered Meal Browsing, Personalized Meal Recommendations powered by gRPC calls to `FitnessCalculationService`. | [Read Docs](./NutritionService/README.md) |
| **6. `WorkoutService`** | `:8091` | `WorkoutDb` | Exercise Catalog, Structured Multi-Week Workout Plans, Category Filtering, Live Workout Session State Machine (`Start` $\rightarrow$ `Complete`), gRPC Session Completion Server. | [Read Docs](./WorkoutService/README.md) |
| **7. `ProgressTrackingService`** | `:8090` | `ProgressTrackingDb` | Weight & Body Measurement Time-Series, Workout Calorie Expenditure, UTC Midnight Streak Engine with Read-Time Decay, Gamified Milestone Achievements, Redis-Cached Dashboards. | [Read Docs](./ProgressTrackingService/README.md) |
| **8. `SubscriptionBillingService`** | `:8092` | `SubscriptionBillingDb` | Native Paymob Unified Intention Checkout, 20-Field Constant-Time HMAC-SHA512 Webhook Processor, Atomic Outbox Auto-Refunds on Race Conditions, Expiration & Refund Sweepers, gRPC Subscription Server. | [Read Docs](./SubscriptionBillingService/README.md) |

---

## 9. Getting Started (Docker Compose & Local Test Suite)

### 9.1 Prerequisites
- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (with Docker Compose v2)

### 9.2 Generate RSA 2048-Bit Keypair (First-Time Setup)
Because `Keys/` is gitignored for security, generate a local RSA keypair in `./Keys` before starting Docker Compose (note: automated tests generate ephemeral keys automatically via `TestRsaKeyHelper`):

```bash
mkdir -p Keys
openssl genpkey -algorithm RSA -out Keys/auth-service-private.pem -pkeyopt rsa_keygen_bits:2048
openssl rsa -pubout -in Keys/auth-service-private.pem -out Keys/auth-service-public.pem
```

### 9.3 Launch the Entire Platform with Docker Compose
```bash
docker compose up --build -d
```
Once all containers report healthy:
- **API Gateway Entrypoint:** `http://localhost:5000`
- **RabbitMQ Management UI:** `http://localhost:15672` (`guest` / `guest`)
- **Swagger UIs (Development Mode):**
  - `AuthenticationService`: `http://localhost:8089/swagger`
  - `UserProfileService`: `http://localhost:8080/swagger`
  - `FitnessCalculationService`: `http://localhost:8081/swagger`
  - `NutritionService`: `http://localhost:8085/swagger`
  - `ProgressTrackingService`: `http://localhost:8090/swagger`
  - `WorkoutService`: `http://localhost:8091/swagger`
  - `SubscriptionBillingService`: `http://localhost:8092/swagger`

### 9.4 Run the Full 225-Test Suite & Code Coverage
```bash
dotnet test FitnessApp.slnx -c Release --logger "trx;LogFileName=test_results.trx" --collect:"XPlat Code Coverage"
```

```text
Passed!  - Failed: 0, Passed:   5, Skipped: 0 - ApiGateway.dll (net9.0)
Passed!  - Failed: 0, Passed:  12, Skipped: 0 - AuthenticationService.dll (net9.0)
Passed!  - Failed: 0, Passed:  53, Skipped: 0 - FitnessCalculationService.dll (net9.0)
Passed!  - Failed: 0, Passed: 119, Skipped: 0 - ProgressTrackingService.dll (net9.0)
Passed!  - Failed: 0, Passed:  36, Skipped: 0 - SubscriptionBillingService.dll (net9.0)
--------------------------------------------------------------------------------------
Total: 225 Passed, 0 Failed, 0 Skipped
```
