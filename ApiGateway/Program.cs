using System.Security.Cryptography;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Ocelot.Provider.Polly;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// 1. Serilog Structured Logging
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] ({CorrelationId}) {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

builder.Host.UseSerilog();

// 2. Ocelot Configuration Files
builder.Configuration
    .SetBasePath(builder.Environment.ContentRootPath)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddJsonFile("ocelot.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"ocelot.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

// 3. Centralized CORS Policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("GatewayCorsPolicy", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// 4. RS256 JWT Authentication with Public Key
var rawPublicKeyPath = builder.Configuration["Jwt:PublicKeyPath"] ?? "Keys/auth-service-public.pem";
var resolvedPublicKeyPath = rawPublicKeyPath;

if (!File.Exists(resolvedPublicKeyPath))
{
    var candidatePaths = new[]
    {
        Path.Combine(builder.Environment.ContentRootPath, rawPublicKeyPath),
        Path.Combine(builder.Environment.ContentRootPath, "../Keys/auth-service-public.pem"),
        Path.Combine(AppContext.BaseDirectory, "keys/auth-service-public.pem"),
        "/app/keys/auth-service-public.pem"
    };

    foreach (var candidate in candidatePaths)
    {
        if (File.Exists(candidate))
        {
            resolvedPublicKeyPath = candidate;
            break;
        }
    }
}

if (File.Exists(resolvedPublicKeyPath))
{
    var rsa = RSA.Create();
    rsa.ImportFromPem(File.ReadAllText(resolvedPublicKeyPath));
    var rsaSecurityKey = new RsaSecurityKey(rsa)
    {
        KeyId = builder.Configuration["Jwt:KeyId"] ?? "elevate-fitness-key-1"
    };

    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer("Bearer", options =>
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
                ValidAudience = builder.Configuration["Jwt:Audience"] ?? "ElevateFitnessUsers"
            };
        });
}
else
{
    Log.Warning("JWT Public key file not found at '{Path}'. Protected routes may reject tokens until key is mounted.", resolvedPublicKeyPath);
}

builder.Services.AddAuthorization();
builder.Services.AddHealthChecks();

// 5. Ocelot + Polly Provider
builder.Services.AddOcelot(builder.Configuration)
    .AddPolly();

var app = builder.Build();

// 6. Request Correlation ID Middleware
app.Use(async (context, next) =>
{
    var correlationId = context.Request.Headers["X-Correlation-ID"].FirstOrDefault()
                        ?? Guid.NewGuid().ToString();

    context.Response.Headers["X-Correlation-ID"] = correlationId;

    using (Serilog.Context.LogContext.PushProperty("CorrelationId", correlationId))
    {
        await next();
    }
});

app.UseSerilogRequestLogging();
app.UseCors("GatewayCorsPolicy");

app.UseAuthentication();
app.UseAuthorization();

// 7. Gateway Health Endpoint
app.MapHealthChecks("/health");

app.MapGet("/", () => Results.Ok(new
{
    Service = "Elevate Fitness API Gateway",
    Status = "Healthy",
    Engine = "Ocelot Reverse Proxy",
    Timestamp = DateTime.UtcNow
}));

// 8. Ocelot Pipeline Execution
await app.UseOcelot();

app.Run();
