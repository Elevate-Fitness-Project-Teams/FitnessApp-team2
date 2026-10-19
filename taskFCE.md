# FCE Service — Task List (Single Project · Vertical Slice · MassTransit)

> Tracks progress for the [Implementation Plan](file:///C:/Users/ezzat/.gemini/antigravity-ide/brain/0ddba4b8-d9c2-4eb7-ba9e-fe7e58785d23/implementation_plan.md).

---

## Phase 1: Project Scaffolding
- [x] Create `FitnessCalculationEngine` project (`dotnet new webapi`)
- [x] Install NuGet packages: MediatR, FluentValidation.AspNetCore, MassTransit, MassTransit.RabbitMQ, EF Core SqlServer, EF Core Tools, Mapster, Serilog.AspNetCore, Swashbuckle.AspNetCore
- [x] Create folder structure: `Common/`, `Domain/`, `Persistence/`, `Features/`, `Consumers/`, `Contracts/`
- [x] Verify project builds: `dotnet build`

---

## Phase 2: Shared Infrastructure (Common + Domain + Persistence)

### Common
- [x] Create `Common/Models/ApiResponse.cs` — unified response envelope
- [x] Create `Common/Behaviors/ValidationBehavior.cs` — MediatR pipeline behavior
- [x] Create `Common/Middleware/GlobalExceptionMiddleware.cs` — exception → error envelope
- [x] Create `Common/Exceptions/NotFoundException.cs`
- [x] Create `Common/Exceptions/BusinessRuleException.cs`

### Domain — Enums
- [x] Create `Domain/Enums/Gender.cs` (Male, Female)
- [x] Create `Domain/Enums/FitnessGoal.cs` (5 values + `[Description]` attributes)
- [x] Create `Domain/Enums/ActivityLevel.cs` (5 values + `ToFactor()` extension)
- [x] Create `Domain/Enums/CalorieStatus.cs` (Weak, Normal, Hard)

### Domain — Entities
- [x] Create `Domain/Entities/UserFitnessStats.cs`
- [x] Create `Domain/Entities/CalculatedMetrics.cs`
- [x] Create `Domain/Entities/FitnessPlanConfig.cs`
- [x] Create `Domain/Entities/UserAssignedPlan.cs`
- [x] Create `Domain/Entities/UserPlanHistory.cs`

### Domain — Services
- [x] Create `Domain/Services/IMetabolicCalculator.cs` interface
- [x] Create `Domain/Services/MetabolicCalculator.cs`:
  - [x] `CalculateBmr(weight, height, age, gender)`
  - [x] `CalculateTdee(bmr, activityLevel)`
  - [x] `CalculateCalorieTarget(tdee, goal)`
  - [x] `ClassifyStatus(calorieTarget)`

### Persistence
- [x] Create `Persistence/FceDbContext.cs` with 5 DbSets
- [x] Create `Persistence/Configurations/UserFitnessStatsConfiguration.cs`
- [x] Create `Persistence/Configurations/CalculatedMetricsConfiguration.cs` (+ concurrency token)
- [x] Create `Persistence/Configurations/FitnessPlanConfigConfiguration.cs` (composite index)
- [x] Create `Persistence/Configurations/UserAssignedPlanConfiguration.cs` (FK + filtered index)
- [x] Create `Persistence/Configurations/UserPlanHistoryConfiguration.cs`
- [x] Create `Persistence/Seed/FitnessPlanConfigSeed.cs` — all 15 Goal×Status rows
- [x] Generate initial EF Core migration
- [x] Verify migration applies cleanly

---

## Phase 3: Feature Slices — Commands

### Slice: SaveFitnessStats
- [x] Create `Features/FitnessStats/SaveFitnessStats.cs`:
  - [x] `SaveFitnessStatsCommand` + `Response` + `Validator` + `Handler` + `Endpoint`

### Slice: CalculateMetrics
- [x] Create `Features/Calculations/CalculateMetrics.cs`:
  - [x] `CalculateMetricsCommand` + `Response` + `Handler` + `Endpoint`

### Slice: AssignPlan
- [x] Create `Features/Plans/AssignPlan.cs`:
  - [x] `AssignPlanCommand` + `Response` + `Handler` (deactivate old → archive → assign new) + `Endpoint`

### Slice: RecalculateMetrics
- [x] Create `Features/Calculations/RecalculateMetrics.cs`:
  - [x] `RecalculateMetricsCommand` + `Response` + `Handler` (update weight → recalculate → conditional reassignment + idempotency check) + `Endpoint`

---

## Phase 4: Feature Slices — Queries

- [x] Create `Features/Calculations/GetUserMetrics.cs` — `GET /api/v1/fitness/metrics/{userId}`
- [x] Create `Features/FitnessStats/GetFitnessStats.cs` — `GET /api/v1/fitness/stats/{userId}`
- [x] Create `Features/Plans/GetPlanConfigs.cs` — `GET /api/v1/fitness/plan-configs`
- [x] Create `Features/Plans/GetPlanById.cs` — `GET /api/v1/fitness/plans/{planId}`
- [x] Create `Features/Plans/GetActivePlan.cs` — `GET /api/v1/fitness/plans/active/{userId}`
- [x] Create `Features/Plans/GetPlanHistory.cs` — `GET /api/v1/fitness/plan-history/{userId}`

---

## Phase 5: Endpoint Registration & Program.cs
- [x] Configure `Program.cs`:
  - [x] Register `FceDbContext` with EF Core
  - [x] Register MediatR (assembly scan)
  - [x] Register FluentValidation validators (assembly scan)
  - [x] Register `ValidationBehavior` in MediatR pipeline
  - [x] Register `IMetabolicCalculator` as singleton
  - [x] Configure MassTransit + RabbitMQ (add consumer, receive endpoint, retry policy)
  - [x] Configure Swagger/OpenAPI
  - [x] Configure CORS
  - [x] Configure JWT authentication
  - [x] Configure Serilog
  - [x] Add `GlobalExceptionMiddleware`
- [x] Map all 8 feature slice endpoints
- [x] Add `appsettings.json` with ConnectionStrings + RabbitMQ config
- [x] Verify API starts and Swagger shows all 8 endpoints

---

## Phase 6: MassTransit Consumer & Transactional Outbox
- [x] Create wire contract `MessageBroker/Events/WeightUpdatedEvent.cs`
- [x] Create `MessageBroker/Consumers/WeightUpdatedConsumer.cs` (`IConsumer<WeightUpdatedEvent>` → dispatches `RecalculateMetricsOrchestrator`)
- [x] Configure MassTransit EF Core Outbox + Inbox with retry policies in `Program.cs`
- [x] Add `AddMassTransitInboxOutbox` EF Core database migration
- [x] Add outbound domain events (`FitnessMetricsRecalculatedEvent`, `FitnessPlanAssignedEvent`) staged inside UoW
- [x] Verify MassTransit auto-creates queues and exchanges

---

## Phase 7: Polish, Hardening & Cross-Cutting
- [x] Add Redis distributed caching (`Microsoft.Extensions.Caching.StackExchangeRedis`) via resilient `ICacheService` abstraction
- [x] Update `docker-compose.yml` with `redis:7-alpine`, health check, and orchestration dependencies
- [x] Cache-aside on `GET /metrics/{userId}` (`metrics:user:{userId}`, 5 min TTL) with post-commit invalidation in orchestrators
- [x] Cache-aside on Plan Configurations (`plancfg:goal:{goal}:status:{status}` & `plancfg:id:{planId}`, 24h TTL)
- [x] Configure Serilog request logging (`UseSerilogRequestLogging`) and CORS policy (`AllowAll`)
- [x] Add resilient startup database migration retry loop (5 attempts with exponential backoff)
- [x] Enrich all REST endpoints with OpenAPI metadata (`.WithName()`, `.WithSummary()`, `.WithDescription()`, `.WithTags()`, JWT security)
- [x] Add ready-to-run requests for all endpoints in `FitnessCalculationService.http`
- [x] Single-file class isolation across all DTOs and orchestrators
- [x] Verify build: `dotnet build -c Release` (0 warnings, 0 errors)

---

## Phase 8: Verification & Acceptance
- [x] Create comprehensive in-service unit test suite under `FitnessCalculationService/Tests/`:
  - [x] Domain engine tests (`MetabolicCalculatorBmrTests`, `MetabolicCalculatorTdeeTests`, `MetabolicCalculatorTargetTests`, `MetabolicCalculatorStatusTests`, `MetabolicCalculatorWorkedExampleTests`)
  - [x] CQRS Command validator tests (`SubmitFitnessStatsCommandValidatorTests`, `CalculateFitnessMetricsCommandValidatorTests`, `CreateAssignedPlanCommandValidatorTests`, `DeactivateActivePlanCommandValidatorTests`, `AppendPlanHistoryCommandValidatorTests`, `RecalculateMetricsOrchestratorValidatorTests`)
  - [x] Redis caching tests (`RedisCacheServiceTests`, `TestDistributedCache`, `FaultyDistributedCache`)
- [x] Verify all 53 unit tests pass (`dotnet test`)
- [x] Verify complete solution build gate (`dotnet build FitnessApp.slnx -c Release` with 0 errors)


