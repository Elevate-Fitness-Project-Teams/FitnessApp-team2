using System.Reflection;
using System.Security.Cryptography;
using FitnessCalculationService.Common;
using FitnessCalculationService.Common.Behaviors;
using FitnessCalculationService.Common.Errors;
using FitnessCalculationService.Common.Json;
using FitnessCalculationService.Common.Middleware;
using FitnessCalculationService.Common.Security;
using FitnessCalculationService.Domain.Enums;
using FitnessCalculationService.Domain.Services;
using FitnessCalculationService.Features.CalculateFitnessMetrics.Commands;
using FitnessCalculationService.Features.Calculations.Queries.GetUserMetrics;
using FitnessCalculationService.Features.FitnessStats.Queries.GetFitnessStats;
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

var builder = WebApplication.CreateBuilder(args);

// Cross-cutting Services
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, CurrentUser>();

// Database & Unit of Work
builder.Services.AddDbContext<FceDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

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
builder.Services.AddOpenApi();
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

// Asynchronous Event-Driven Messaging (MassTransit + RabbitMQ)
builder.Services.AddMassTransit(x =>
{
    x.SetKebabCaseEndpointNameFormatter();
    x.AddConsumer<WeightUpdatedConsumer>();

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

app.MapOpenApi();
app.MapHealthChecks("/health");

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();

// gRPC Endpoints
app.MapGrpcService<CalorieTargetGrpcService>();

// REST Endpoints
GetUserMetricsEndpoint.Map(app);
GetFitnessStatsEndpoint.Map(app);
SubmitFitnessStatsEndpoint.Map(app);
CalculateFitnessMetricsEndpoint.Map(app);

// Automatically apply any pending EF Core migrations on startup
using (var scope = app.Services.CreateScope())
{
    try
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<FceDbContext>();
        await dbContext.Database.MigrateAsync();
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while applying database migrations.");
        throw;
    }
}

app.Run();
