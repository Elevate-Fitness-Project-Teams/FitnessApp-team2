using System.Security.Cryptography;
using FluentValidation;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;
using SubscriptionBillingService.Common.Behaviors;
using SubscriptionBillingService.Common.Database;
using SubscriptionBillingService.Features.Subscriptions.Consumers;
using SubscriptionBillingService.Infrastructure.Caching;
using SubscriptionBillingService.Infrastructure.Paymob;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// MediatR & Pipeline Behaviors
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
    cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
});
builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);

// Paymob Options & Gateway HTTP Client
builder.Services.Configure<PaymobOptions>(builder.Configuration.GetSection(PaymobOptions.SectionName));
builder.Services.AddHttpClient<IPaymobGatewayClient, PaymobGatewayClient>((sp, client) =>
{
    var options = builder.Configuration.GetSection(PaymobOptions.SectionName).Get<PaymobOptions>() ?? new PaymobOptions();
    client.BaseAddress = new Uri(options.BaseUrl);
    client.Timeout = TimeSpan.FromSeconds(15);
});

builder.Services.AddSingleton(sp =>
{
    var hmacSecret = builder.Configuration["Paymob:HmacSecret"] ?? string.Empty;
    return new PaymobHmacValidator(hmacSecret);
});

// Redis Caching & Shared Connection Multiplexer
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var redisConfig = builder.Configuration["Redis:ConnectionString"] ?? "localhost:6379";
    var options = ConfigurationOptions.Parse(redisConfig);
    options.AbortOnConnectFail = false;
    options.ConnectTimeout = 1000;
    options.SyncTimeout = 500;
    options.AsyncTimeout = 500;
    return ConnectionMultiplexer.Connect(options);
});
builder.Services.AddSingleton<ISubscriptionTierCache, RedisSubscriptionTierCache>();

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration["Redis:ConnectionString"] ?? "localhost:6379";
    options.InstanceName = "sub:";
});

// MassTransit with Entity Framework Core Outbox & RabbitMQ
builder.Services.AddMassTransit(x =>
{
    x.SetKebabCaseEndpointNameFormatter();

    x.AddConsumer<ProcessPaymobRefundConsumer>();

    x.AddEntityFrameworkOutbox<SubscriptionDbContext>(o =>
    {
        o.UseSqlServer();
        o.UseBusOutbox();
        o.DuplicateDetectionWindow = TimeSpan.FromMinutes(30);
    });

    x.UsingRabbitMq((context, cfg) =>
    {
        var rabbitMqConfig = builder.Configuration.GetSection("RabbitMq");
        var host = rabbitMqConfig["Host"] ?? "localhost";
        var virtualHost = rabbitMqConfig["VirtualHost"] ?? "/";
        var username = rabbitMqConfig["Username"] ?? "guest";
        var password = rabbitMqConfig["Password"] ?? "guest";

        cfg.Host(host, virtualHost, h =>
        {
            h.Username(username);
            h.Password(password);
        });

        cfg.ReceiveEndpoint("subscription-refund-processor", ep =>
        {
            ep.UseMessageRetry(r => r.Exponential(5, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(2)));
            ep.ConfigureConsumer<ProcessPaymobRefundConsumer>(context);
        });

        cfg.ConfigureEndpoints(context);
    });
});

// Database & Persistence
builder.Services.AddDbContext<SubscriptionDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    if (!string.IsNullOrEmpty(connectionString))
    {
        options.UseSqlServer(connectionString);
    }
});

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

// RS256 JWT Authentication
var publicKeyPath = builder.Configuration["Jwt:PublicKeyPath"] ?? "../Keys/auth-service-public.pem";
RsaSecurityKey rsaSecurityKey;
if (File.Exists(publicKeyPath))
{
    var rsa = RSA.Create();
    rsa.ImportFromPem(File.ReadAllText(publicKeyPath));
    rsaSecurityKey = new RsaSecurityKey(rsa)
    {
        KeyId = builder.Configuration["Jwt:KeyId"] ?? "elevate-fitness-key-1"
    };
}
else
{
    var rsa = RSA.Create(2048);
    rsaSecurityKey = new RsaSecurityKey(rsa)
    {
        KeyId = builder.Configuration["Jwt:KeyId"] ?? "dev-key"
    };
}

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
            ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "ElevateFitness",
            ValidAudience = builder.Configuration["Jwt:Audience"] ?? "ElevateFitnessUsers",
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });

builder.Services.AddAuthorization();

// OpenAPI documentation
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        var scheme = new Microsoft.OpenApi.Models.OpenApiSecurityScheme
        {
            Name = "Authorization",
            Description = "Enter JWT Bearer token: Bearer {token}",
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

// Background Sweepers
builder.Services.AddHostedService<SubscriptionBillingService.Infrastructure.Workers.SubscriptionExpirationSweeper>();
builder.Services.AddHostedService<SubscriptionBillingService.Infrastructure.Workers.PendingRefundSweeper>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "Subscription & Billing API v1");
    });
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();

public partial class Program { }
