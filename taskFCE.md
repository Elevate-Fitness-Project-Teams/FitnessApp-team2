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
- [ ] Create `Features/FitnessStats/SaveFitnessStats.cs`:
  - [ ] `SaveFitnessStatsCommand` + `Response` + `Validator` + `Handler` + `Endpoint`

### Slice: CalculateMetrics
- [ ] Create `Features/Calculations/CalculateMetrics.cs`:
  - [ ] `CalculateMetricsCommand` + `Response` + `Handler` + `Endpoint`

### Slice: AssignPlan
- [ ] Create `Features/Plans/AssignPlan.cs`:
  - [ ] `AssignPlanCommand` + `Response` + `Handler` (deactivate old → archive → assign new) + `Endpoint`

### Slice: RecalculateMetrics
- [ ] Create `Features/Calculations/RecalculateMetrics.cs`:
  - [ ] `RecalculateMetricsCommand` + `Response` + `Handler` (update weight → recalculate → conditional reassignment + idempotency check) + `Endpoint`

---

## Phase 4: Feature Slices — Queries

- [x] Create `Features/Calculations/GetUserMetrics.cs` — `GET /api/v1/fitness/metrics/{userId}`
- [x] Create `Features/FitnessStats/GetFitnessStats.cs` — `GET /api/v1/fitness/stats/{userId}`
- [ ] Create `Features/Plans/GetPlanConfigs.cs` — `GET /api/v1/fitness/plan-configs`
- [ ] Create `Features/Plans/GetPlanById.cs` — `GET /api/v1/fitness/plans/{planId}`

---

## Phase 5: Endpoint Registration & Program.cs
- [ ] Configure `Program.cs`:
  - [ ] Register `FceDbContext` with EF Core
  - [ ] Register MediatR (assembly scan)
  - [ ] Register FluentValidation validators (assembly scan)
  - [ ] Register `ValidationBehavior` in MediatR pipeline
  - [ ] Register `IMetabolicCalculator` as singleton
  - [ ] Configure MassTransit + RabbitMQ (add consumer, receive endpoint, retry policy)
  - [ ] Configure Swagger/OpenAPI
  - [ ] Configure CORS
  - [ ] Configure JWT authentication
  - [ ] Configure Serilog
  - [ ] Add `GlobalExceptionMiddleware`
- [ ] Map all 8 feature slice endpoints
- [ ] Add `appsettings.json` with ConnectionStrings + RabbitMQ config
- [ ] Verify API starts and Swagger shows all 8 endpoints

---

## Phase 6: MassTransit Consumer
- [ ] Create `Contracts/Events/WeightUpdatedEvent.cs`
- [ ] Create `Consumers/WeightUpdatedConsumer.cs` (`IConsumer<WeightUpdatedEvent>` → dispatches `RecalculateMetricsCommand`)
- [ ] Verify MassTransit auto-creates queues and exchanges
- [ ] Test consumer with a manually published message to RabbitMQ

---

## Phase 7: Polish & Hardening
- [ ] Add response caching on `GET /metrics/{userId}`
- [ ] Cache `FitnessPlanConfig` in memory at startup
- [ ] Add `RowVersion` concurrency token on `CalculatedMetrics`
- [ ] Verify all 15 seed data rows present
- [ ] Review Swagger docs for completeness
- [ ] Verify build: `dotnet build -c Release`
