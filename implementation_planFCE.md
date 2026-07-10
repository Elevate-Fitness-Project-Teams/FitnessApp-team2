# FCE Service — Implementation Plan (Vertical Slice + MassTransit)

> **No testing** — this plan excludes unit and integration tests per project requirements.

> [!NOTE]
> This plan implements the Fitness Calculation Engine as defined in the [FCE Engineering Report](file:///C:/Users/ezzat/.gemini/antigravity-ide/brain/0ddba4b8-d9c2-4eb7-ba9e-fe7e58785d23/FCE_Engineering_Report.md).

## Architecture Decisions

| Decision | Choice |
|---|---|
| **Architecture** | **Vertical Slice** — features organized by slice, not by technical layer |
| **Messaging** | **MassTransit** over RabbitMQ transport |
| **CQRS** | MediatR for in-process command/query dispatch |
| **Validation** | FluentValidation wired into MediatR pipeline |
| **ORM** | Entity Framework Core (handlers use `DbContext` directly — no repository abstraction) |
| **Framework** | .NET 8, ASP.NET Core Minimal APIs or Controllers |

---

## Open Questions

1. **Database Provider:** SQL Server, PostgreSQL, or other? (Affects EF Core provider package)
2. **Minimal APIs vs Controllers:** Vertical Slice works with both. Do you prefer `app.MapPost()` style or `[ApiController]` style?
3. **Solution Location:** `d:\work\Fitness\FitnessApp-team2\FitnessCalculationEngine\` — correct?
4. **Docker:** Need `Dockerfile` + `docker-compose.yml` (including RabbitMQ container)?

---

## Proposed Changes

### Phase 1 — Project Scaffolding

Summary: Create a single standalone ASP.NET Core project — one microservice = one project. No solution file, no multi-project layering.

#### [NEW] Project Structure (Single Microservice)

```
FitnessCalculationEngine/
├── FitnessCalculationEngine.csproj              # Single project = the entire microservice
├── Program.cs                                    # Host configuration
├── appsettings.json
│
├── Common/                                       # Shared cross-cutting concerns
│   ├── Models/
│   │   └── ApiResponse.cs                        # Unified response envelope
│   ├── Behaviors/
│   │   └── ValidationBehavior.cs                 # MediatR pipeline
│   ├── Middleware/
│   │   └── GlobalExceptionMiddleware.cs
│   ├── Exceptions/
│   │   ├── NotFoundException.cs
│   │   └── BusinessRuleException.cs
│   └── Extensions/
│       └── ServiceCollectionExtensions.cs        # DI registration helpers
│
├── Domain/                                       # Domain logic
│   ├── Entities/
│   │   ├── UserFitnessStats.cs
│   │   ├── CalculatedMetrics.cs
│   │   ├── FitnessPlanConfig.cs
│   │   ├── UserAssignedPlan.cs
│   │   └── UserPlanHistory.cs
│   ├── Enums/
│   │   ├── Gender.cs
│   │   ├── FitnessGoal.cs
│   │   ├── ActivityLevel.cs
│   │   └── CalorieStatus.cs
│   └── Services/
│       └── MetabolicCalculator.cs                # Pure calculation logic
│
├── Persistence/                                  # EF Core infrastructure
│   ├── FceDbContext.cs
│   ├── Configurations/
│   │   ├── UserFitnessStatsConfiguration.cs
│   │   ├── CalculatedMetricsConfiguration.cs
│   │   ├── FitnessPlanConfigConfiguration.cs
│   │   ├── UserAssignedPlanConfiguration.cs
│   │   └── UserPlanHistoryConfiguration.cs
│   ├── Seed/
│   │   └── FitnessPlanConfigSeed.cs
│   └── Migrations/
│
├── Features/                                     # ⭐ VERTICAL SLICES ⭐
│   ├── FitnessStats/
│   │   ├── SaveFitnessStats.cs                   # Command + Handler + Validator + Endpoint
│   │   └── GetFitnessStats.cs                    # Query + Handler + Endpoint
│   ├── Calculations/
│   │   ├── CalculateMetrics.cs                   # Command + Handler + Endpoint
│   │   ├── RecalculateMetrics.cs                 # Command + Handler + Endpoint
│   │   └── GetUserMetrics.cs                     # Query + Handler + Endpoint
│   └── Plans/
│       ├── AssignPlan.cs                         # Command + Handler + Endpoint
│       ├── GetPlanConfigs.cs                     # Query + Handler + Endpoint
│       └── GetPlanById.cs                        # Query + Handler + Endpoint
│
├── Consumers/                                    # MassTransit event consumers
│   └── WeightUpdatedConsumer.cs
│
└── Contracts/                                    # Integration event DTOs (folder, not separate project)
    └── Events/
        └── WeightUpdatedEvent.cs
```

> [!IMPORTANT]
> **One project = one microservice.** Everything lives inside `FitnessCalculationEngine.csproj`. No solution file, no layered projects. Event contracts live in a `Contracts/` folder within the same project.

> [!TIP]
> **Vertical Slice rule:** Each `.cs` file in `Features/` is self-contained — request record, response DTO, FluentValidation validator, MediatR handler, and endpoint mapping all in **one file**. Handlers use `FceDbContext` directly, no repository abstraction.

**NuGet Packages:**

`MediatR`, `FluentValidation.AspNetCore`, `MassTransit`, `MassTransit.RabbitMQ`, `Microsoft.EntityFrameworkCore.SqlServer`, `Microsoft.EntityFrameworkCore.Tools`, `Mapster`, `Serilog.AspNetCore`, `Swashbuckle.AspNetCore`

---

### Phase 2 — Shared Infrastructure (Common + Domain + Persistence)

Summary: Build the cross-cutting concerns that all slices depend on before writing any feature slices.

---

#### [NEW] `Common/Models/ApiResponse.cs`

Unified response envelope used by all slices:
```csharp
public record ApiResponse<T>(
    bool IsSuccess,
    string Message,
    T? Data,
    List<string> Errors,
    int StatusCode,
    DateTime Timestamp);
```

#### [NEW] `Common/Behaviors/ValidationBehavior.cs`

MediatR pipeline behavior that auto-runs FluentValidation before every handler:
```csharp
public class ValidationBehavior<TRequest, TResponse> 
    : IPipelineBehavior<TRequest, TResponse>
```

#### [NEW] `Common/Middleware/GlobalExceptionMiddleware.cs`

Catches exceptions and maps them to the unified envelope with proper error codes:
| Exception Type | HTTP Status | Error Code |
|---|---|---|
| `ValidationException` | 400 | `VAL_*` |
| `NotFoundException` | 404 | `FCE_STATS_NOT_FOUND`, `FCE_NO_MATCHING_PLAN` |
| `BusinessRuleException` | 400 | `FCE_METRICS_NOT_CALCULATED` |

#### [NEW] `Common/Exceptions/`
- `NotFoundException.cs`
- `BusinessRuleException.cs`

---

#### [NEW] Domain Enums (4 files)

| Enum | Values | Notes |
|---|---|---|
| `Gender` | `Male`, `Female` | |
| `FitnessGoal` | `LoseWeight`, `GetFitter`, `GainWeight`, `GainMoreFlexible`, `LearnTheBasic` | `[Description]` attributes for DB string mapping |
| `ActivityLevel` | `Rookie`, `Beginner`, `Intermediate`, `Advance`, `TrueBeast` | Extension method `ToFactor()` returning multiplier |
| `CalorieStatus` | `Weak`, `Normal`, `Hard` | |

#### [NEW] Domain Entities (5 files)

One entity per table matching the schema from the engineering report.

#### [NEW] `Domain/Services/MetabolicCalculator.cs`

Stateless pure service — registered as `Singleton` in DI:
```csharp
public class MetabolicCalculator : IMetabolicCalculator
{
    public double CalculateBmr(double weight, double height, int age, Gender gender);
    public double CalculateTdee(double bmr, ActivityLevel activityLevel);
    public double CalculateCalorieTarget(double tdee, FitnessGoal goal);
    public CalorieStatus ClassifyStatus(double calorieTarget);
}
```

---

#### [NEW] Persistence (DbContext + 5 Configurations + Seed Data)

| File | Key Config |
|---|---|
| `FceDbContext.cs` | 5 `DbSet<>` properties, `OnModelCreating` applies configs from assembly |
| `UserFitnessStatsConfiguration.cs` | Unique index on `UserId`, max lengths, check constraints |
| `CalculatedMetricsConfiguration.cs` | Unique index on `UserId`, concurrency token (`RowVersion`) |
| `FitnessPlanConfigConfiguration.cs` | Composite index on `(Goal, Status)` |
| `UserAssignedPlanConfiguration.cs` | FK to `FitnessPlanConfig.PlanId`, filtered index `WHERE IsActive = true` |
| `UserPlanHistoryConfiguration.cs` | Index on `(UserId, AssignedAt DESC)` |
| `FitnessPlanConfigSeed.cs` | Seeds 15 rows (5 goals × 3 statuses) |

---

### Phase 3 — Feature Slices: Commands (State-Changing)

Summary: Build the 4 command slices. Each slice file contains everything: request, validator, handler, endpoint.

---

#### [NEW] `Features/FitnessStats/SaveFitnessStats.cs`

**Anatomy of a single vertical slice file:**

```csharp
// === Features/FitnessStats/SaveFitnessStats.cs ===

// 1. Request (Command)
public record SaveFitnessStatsCommand(
    int UserId, double Weight, double Height, int Age,
    string Gender, string Goal, string ActivityLevel
) : IRequest<ApiResponse<SaveFitnessStatsResponse>>;

// 2. Response DTO
public record SaveFitnessStatsResponse(
    int UserId, double Weight, double Height, int Age,
    string Gender, string Goal, string ActivityLevel, DateTime RecordedAt);

// 3. Validator
public class SaveFitnessStatsValidator : AbstractValidator<SaveFitnessStatsCommand>
{
    public SaveFitnessStatsValidator()
    {
        RuleFor(x => x.Age).InclusiveBetween(16, 100);
        RuleFor(x => x.Weight).InclusiveBetween(40, 200);
        RuleFor(x => x.Height).InclusiveBetween(140, 220);
        RuleFor(x => x.Gender).Must(g => g is "Male" or "Female");
        // ... goal + activityLevel validation
    }
}

// 4. Handler
public class SaveFitnessStatsHandler 
    : IRequestHandler<SaveFitnessStatsCommand, ApiResponse<SaveFitnessStatsResponse>>
{
    private readonly FceDbContext _db;
    // Uses DbContext directly — no repository
    
    public async Task<ApiResponse<SaveFitnessStatsResponse>> Handle(...)
    {
        // UPSERT into UserFitnessStats
    }
}

// 5. Endpoint Mapping (called from Program.cs or via extension)
public static class SaveFitnessStatsEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/fitness/weight-goal-activity", async (
            SaveFitnessStatsCommand command, IMediator mediator) =>
        {
            var result = await mediator.Send(command);
            return Results.Ok(result);
        }).RequireAuthorization();
    }
}
```

> [!TIP]
> This is the pattern every slice follows. One file, one feature, zero cross-dependencies between slices.

---

#### [NEW] `Features/Calculations/CalculateMetrics.cs`

| Component | Details |
|---|---|
| Command | `CalculateMetricsCommand(int UserId)` |
| Handler | Fetches `UserFitnessStats` → calls `IMetabolicCalculator` → UPSERT `CalculatedMetrics` |
| Error | Throws `NotFoundException` if no stats → maps to `FCE_STATS_NOT_FOUND` (404) |

---

#### [NEW] `Features/Plans/AssignPlan.cs`

| Component | Details |
|---|---|
| Command | `AssignPlanCommand(int UserId)` |
| Handler Logic | 1. Fetch `CalculatedMetrics` (throw if missing)<br/>2. Query `FitnessPlanConfig WHERE Goal AND Status`<br/>3. If active plan exists: `IsActive = false` + insert `UserPlanHistory`<br/>4. Insert new `UserAssignedPlan` with `IsActive = true` |
| Errors | `FCE_METRICS_NOT_CALCULATED` (400), `FCE_NO_MATCHING_PLAN` (404) |

---

#### [NEW] `Features/Calculations/RecalculateMetrics.cs`

| Component | Details |
|---|---|
| Command | `RecalculateMetricsCommand(int UserId, double? NewWeight, string? Reason, string? TriggeredBy)` |
| Handler Logic | 1. Update weight if provided<br/>2. Snapshot previous metrics<br/>3. Re-run full pipeline<br/>4. Compare status → conditional plan reassignment<br/>5. Update `CalculatedMetrics` |
| Key | This is also invoked by the MassTransit `WeightUpdatedConsumer` |

---

### Phase 4 — Feature Slices: Queries (Read-Only)

Summary: Build the 4 query slices.

#### [NEW] `Features/Calculations/GetUserMetrics.cs`

| Component | Details |
|---|---|
| Query | `GetUserMetricsQuery(int UserId)` |
| Handler | Reads `CalculatedMetrics` by `UserId` |
| Endpoint | `GET /api/v1/fitness/metrics/{userId}` |
| Note | **High-traffic endpoint** — called by Nutrition + Smart Coach services |

#### [NEW] `Features/FitnessStats/GetFitnessStats.cs`

| Component | Details |
|---|---|
| Query | `GetUserFitnessStatsQuery(int UserId)` |
| Endpoint | `GET /api/v1/fitness/stats/{userId}` |

#### [NEW] `Features/Plans/GetPlanConfigs.cs`

| Component | Details |
|---|---|
| Query | `GetPlanConfigsQuery` (no params) |
| Endpoint | `GET /api/v1/fitness/plan-configs` |
| Note | Returns all rows from `FitnessPlanConfig` — consider in-memory caching |

#### [NEW] `Features/Plans/GetPlanById.cs`

| Component | Details |
|---|---|
| Query | `GetPlanByIdQuery(string PlanId)` |
| Endpoint | `GET /api/v1/fitness/plans/{planId}` |
| Error | `FCE_NO_MATCHING_PLAN` (404) |

---

### Phase 5 — Endpoint Registration & Program.cs

Summary: Wire up all slices, MediatR, FluentValidation, EF Core, MassTransit, and middleware in `Program.cs`.

#### [MODIFY] `Program.cs`

```csharp
var builder = WebApplication.CreateBuilder(args);

// EF Core
builder.Services.AddDbContext<FceDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("FceDb")));

// MediatR + FluentValidation pipeline
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));
builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

// Domain services
builder.Services.AddSingleton<IMetabolicCalculator, MetabolicCalculator>();

// MassTransit + RabbitMQ
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<WeightUpdatedConsumer>();
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration["RabbitMQ:Host"], h =>
        {
            h.Username(builder.Configuration["RabbitMQ:Username"]);
            h.Password(builder.Configuration["RabbitMQ:Password"]);
        });
        cfg.ReceiveEndpoint("fce-weight-updated", e =>
        {
            e.ConfigureConsumer<WeightUpdatedConsumer>(context);
            e.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(5)));
        });
    });
});

// Swagger, Auth, CORS, Serilog...

var app = builder.Build();

app.UseMiddleware<GlobalExceptionMiddleware>();

// Map all feature slice endpoints
SaveFitnessStatsEndpoint.Map(app);
CalculateMetricsEndpoint.Map(app);
AssignPlanEndpoint.Map(app);
RecalculateMetricsEndpoint.Map(app);
GetUserMetricsEndpoint.Map(app);
GetFitnessStatsEndpoint.Map(app);
GetPlanConfigsEndpoint.Map(app);
GetPlanByIdEndpoint.Map(app);

app.Run();
```

---

### Phase 6 — MassTransit Consumer (Async Messaging)

Summary: Implement the `weight_updated` event consumer using MassTransit's `IConsumer<T>` pattern.

#### [NEW] `Contracts/Events/WeightUpdatedEvent.cs`

```csharp
namespace FitnessCalculationEngine.Contracts.Events;

public record WeightUpdatedEvent(int UserId, double NewWeight, DateTime RecordedAt);
```

#### [NEW] `Consumers/WeightUpdatedConsumer.cs`

```csharp
public class WeightUpdatedConsumer : IConsumer<WeightUpdatedEvent>
{
    private readonly IMediator _mediator;
    private readonly ILogger<WeightUpdatedConsumer> _logger;

    public async Task Consume(ConsumeContext<WeightUpdatedEvent> context)
    {
        var evt = context.Message;
        _logger.LogInformation("Received weight_updated for User {UserId}", evt.UserId);

        await _mediator.Send(new RecalculateMetricsCommand(
            UserId: evt.UserId,
            NewWeight: evt.NewWeight,
            Reason: "weight_update",
            TriggeredBy: "progress_service"
        ));
    }
}
```

**MassTransit handles automatically:**
- Message deserialization
- Retry policies (configured in `Program.cs`)
- Dead-letter / error queue (`fce-weight-updated_error`)
- Graceful shutdown
- Scoped DI resolution (each message gets its own scope)

> [!TIP]
> **Idempotency:** Add a check in `RecalculateMetricsHandler` — if `CalculatedMetrics.CalculatedAt > event.RecordedAt`, skip the recalculation (stale event).

---

### Phase 7 — Polish & Hardening

Summary: Final verification, performance considerations, and documentation.

- Add response caching on `GET /metrics/{userId}` (short TTL, invalidate on recalculation)
- Cache `FitnessPlanConfig` in memory at startup
- Add concurrency token (`RowVersion`) on `CalculatedMetrics` to prevent race conditions
- Review Swagger documentation for completeness
- Verify all 15 seed data rows exist in `FitnessPlanConfig`

---

## Verification Plan

### Build Verification
```bash
dotnet build src/FCE.API -c Release --no-restore
```

### Manual Verification
1. Run API: `dotnet run --project src/FCE.API`
2. Open Swagger at `https://localhost:{port}/swagger`
3. Test full onboarding flow through Swagger
4. Publish a test `WeightUpdatedEvent` to RabbitMQ → verify recalculation happens
5. Verify all error paths return correct error codes and HTTP statuses
