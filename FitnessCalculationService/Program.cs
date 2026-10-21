using System.Reflection;
using System.Security.Cryptography;
using FitnessCalculationService.Common;
using FitnessCalculationService.Common.Behaviors;
using FitnessCalculationService.Common.Caching;
using FitnessCalculationService.Common.Errors;
using FitnessCalculationService.Common.Json;
using FitnessCalculationService.Common.Middleware;
using FitnessCalculationService.Common.Security;
using FitnessCalculationService.Domain.Enums;
using FitnessCalculationService.Domain.Services;
using FitnessCalculationService.Features.CalculateFitnessMetrics.Commands;
using FitnessCalculationService.Features.Calculations.Commands.RecalculateMetrics;
using FitnessCalculationService.Features.Calculations.Queries.GetUserMetrics;
using FitnessCalculationService.Features.FitnessStats.Queries.GetFitnessStats;
using FitnessCalculationService.Features.Plans;
using FitnessCalculationService.Features.Plans.Queries.GetActivePlan;
using FitnessCalculationService.Features.Plans.Queries.GetPlanById;
using FitnessCalculationService.Features.Plans.Queries.GetPlanConfigs;
using FitnessCalculationService.Features.Plans.Queries.GetPlanHistory;
using FitnessCalculationService.Features.SubmitWeightGoalActivity.Commands;
using FitnessCalculationService.GrpcServices;
using FitnessCalculationService.MessageBroker.Consumers;
using FitnessCalculationService.Persistence;
using FitnessCalculationService.Persistence.Repositories;
using FluentValidation;
using MassTransit;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Serilog Structured Logging Host Configuration
builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext()
    .WriteTo.Console());

// Cross-cutting Services
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, CurrentUser>();

// Database & Unit of Work
builder.Services.AddDbContext<FceDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

// Redis Distributed Caching Layer
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration["Redis:ConnectionString"] ?? "localhost:6379";
    options.InstanceName = "fce:";
});
builder.Services.AddSingleton<ICacheService, RedisCacheService>();

// CORS Configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Domain Services
builder.Services.AddSingleton<IMetabolicCalculator, MetabolicCalculator>();

// MediatR & FluentValidation Pipeline
builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
    cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
});

// JSON Serialization Configuration
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new EnumDescriptionConverter<FitnessGoal>());
    options.SerializerOptions.Converters.Add(new EnumDescriptionConverter<Gender>());
    options.SerializerOptions.Converters.Add(new EnumDescriptionConverter<ActivityLevel>());
    options.SerializerOptions.Converters.Add(new EnumDescriptionConverter<CalorieStatus>());
});

// High-Performance gRPC for Synchronous Inter-Service Communication (e.g. NutritionService, SmartCoach)
builder.Services.AddGrpc();

// OpenAPI & Health Checks
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        var scheme = new Microsoft.OpenApi.Models.OpenApiSecurityScheme
        {
            Name = "Authorization",
            Description = "Enter JWT Bearer token format: Bearer {token}",
            In = Microsoft.OpenApi.Models.ParameterLocation.Header,
            Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT"
        };

        document.Components ??= new Microsoft.OpenApi.Models.OpenApiComponents();
        document.Components.SecuritySchemes.Add("Bearer", scheme);

        var requirement = new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
        {
            {
                new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                {
                    Reference = new Microsoft.OpenApi.Models.OpenApiReference
                    {
                        Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        };

        document.SecurityRequirements.Add(requirement);
        return Task.CompletedTask;
    });
});
builder.Services.AddHealthChecks();

// RS256 JWT Authentication with Public Key
var publicKeyPath = builder.Configuration["Jwt:PublicKeyPath"]
    ?? throw new InvalidOperationException("JWT PublicKeyPath is not configured");

var rsa = RSA.Create();
rsa.ImportFromPem(File.ReadAllText(publicKeyPath));
var rsaSecurityKey = new RsaSecurityKey(rsa)
{
    KeyId = builder.Configuration["Jwt:KeyId"]
};

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            IssuerSigningKey = rsaSecurityKey,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"]
        };

        options.Events = new JwtBearerEvents
        {
            OnChallenge = async context =>
            {
                context.HandleResponse();
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                context.Response.ContentType = "application/json";

                var isExpired = context.AuthenticateFailure is SecurityTokenExpiredException;
                var errorCode = isExpired ? ErrorCodes.AuthTokenExpired : ErrorCodes.AuthTokenInvalid;
                var message = isExpired ? "Authentication token has expired." : "Authentication token is missing or malformed.";

                var failureResponse = ApiResponse<object?>.Failure(
                    new[] { errorCode },
                    message,
                    StatusCodes.Status401Unauthorized);

                await context.Response.WriteAsJsonAsync(failureResponse);
            }
        };
    });

builder.Services.AddAuthorization();

// Asynchronous Event-Driven Messaging (MassTransit + RabbitMQ + Transactional Outbox/Inbox)
builder.Services.AddMassTransit(x =>
{
    x.SetKebabCaseEndpointNameFormatter();
    x.AddConsumer<WeightUpdatedConsumer>();

    x.AddEntityFrameworkOutbox<FceDbContext>(o =>
    {
        o.UseSqlServer();
        o.UseBusOutbox();
        o.QueryDelay = TimeSpan.FromSeconds(5);
        o.DuplicateDetectionWindow = TimeSpan.FromMinutes(30);
    });

    x.AddConfigureEndpointsCallback((ctx, name, cfg) =>
    {
        cfg.UseMessageRetry(r => r.Exponential(5, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(2)));
        cfg.UseEntityFrameworkOutbox<FceDbContext>(ctx);
    });

    x.UsingRabbitMq((context, cfg) =>
    {
        var rabbitMqConfig = builder.Configuration.GetSection("RabbitMq");
        var host = rabbitMqConfig["Host"] ?? throw new InvalidOperationException("RabbitMq:Host is not configured.");
        var virtualHost = rabbitMqConfig["VirtualHost"] ?? "/";
        var username = rabbitMqConfig["Username"] ?? throw new InvalidOperationException("RabbitMq:Username is not configured.");
        var password = rabbitMqConfig["Password"] ?? throw new InvalidOperationException("RabbitMq:Password is not configured.");

        cfg.Host(host, virtualHost, h =>
        {
            h.Username(username);
            h.Password(password);
        });

        cfg.ConfigureEndpoints(context);
    });
});

var app = builder.Build();

// 1. Global Exception Middleware MUST be the first middleware to capture all pipeline errors
app.UseMiddleware<GlobalExceptionMiddleware>();

// 2. Serilog Structured Request Logging
app.UseSerilogRequestLogging();

app.MapOpenApi();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/openapi/v1.json", "v1");
    options.RoutePrefix = "swagger";
});
app.MapGet("/", () => Results.Redirect("/swagger"));
app.MapHealthChecks("/health");

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();

// gRPC Endpoints
app.MapGrpcService<CalorieTargetGrpcService>();

// REST Endpoints
GetUserMetricsEndpoint.Map(app);
GetFitnessStatsEndpoint.Map(app);
SubmitFitnessStatsEndpoint.Map(app);
CalculateFitnessMetricsEndpoint.Map(app);
AssignPlanEndpoint.Map(app);
RecalculateMetricsEndpoint.Map(app);
GetPlanConfigsEndpoint.Map(app);
GetPlanByIdEndpoint.Map(app);
GetActivePlanEndpoint.Map(app);
GetPlanHistoryEndpoint.Map(app);

// Automatically apply any pending EF Core migrations on startup with resilient retry
using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    var dbContext = scope.ServiceProvider.GetRequiredService<FceDbContext>();
    var maxRetries = 5;
    var retryDelay = TimeSpan.FromSeconds(3);

    for (var attempt = 1; attempt <= maxRetries; attempt++)
    {
        try
        {
            logger.LogInformation("Applying pending EF Core database migrations (Attempt {Attempt}/{MaxRetries})...", attempt, maxRetries);
            await dbContext.Database.MigrateAsync();
            logger.LogInformation("Database migrations applied successfully.");

            // Seed baseline test stats and active plan for development and testing
            await FitnessCalculationService.Persistence.Seed.FitnessCalculationDbSeeder.SeedAsync(app.Services);
            break;
        }
        catch (Exception ex) when (attempt < maxRetries)
        {
            logger.LogWarning(ex, "Failed to apply database migrations on attempt {Attempt}. Retrying in {Delay}s...", attempt, retryDelay.TotalSeconds);
            await Task.Delay(retryDelay);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unrecoverable error occurred while applying database migrations after {MaxRetries} attempts.", maxRetries);
            throw;
        }
    }
}

app.Run();
