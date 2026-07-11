using FluentValidation;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ProgressTrackingService.Common.Behaviors;
using ProgressTrackingService.Common.Database;
using ProgressTrackingService.Data;
using System.Security.Cryptography;
using ProgressTrackingService.Features.WorkoutLogs.Orchestrators.LogWorkout;
using ProgressTrackingService.Features.Progress.Queries.ViewUserProgress;
using ProgressTrackingService.Features.Progress.Commands.LogWeight;
using ProgressTrackingService.MessageBroker.Consumers;
using ProgressTrackingService.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
    cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
});

builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);


builder.Services.AddMassTransit(x =>
{
    x.SetKebabCaseEndpointNameFormatter();
    x.AddConsumer<UserRegisteredConsumer>();

    x.AddEntityFrameworkOutbox<ApplicationDbContext>(o =>
    {
        o.UseSqlServer();
        o.UseBusOutbox();
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
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
    options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
});

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped(typeof(IGeneralRepo<>), typeof(GeneralRepo<>));

builder.Services.AddHttpClient("WorkoutService", client =>
{
    var baseUrl = builder.Configuration["WorkoutService:BaseUrl"] ?? "http://workoutservice";
    client.BaseAddress = new Uri(baseUrl);
});

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// RS256 JWT validation with public key
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
    });

builder.Services.AddAuthorization();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        var origins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
        policy.WithOrigins(origins)
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();


#region ApplyPendingMigration

using var scopeApplicationContext = app.Services.CreateScope();
var context = scopeApplicationContext.ServiceProvider.GetRequiredService<ApplicationDbContext>();
try
{
    await context.Database.MigrateAsync();
}
catch (Exception e)
{
    var logger = scopeApplicationContext.ServiceProvider.GetRequiredService<ILogger<Program>>();
    logger.LogError(e, "An error occurred while migrating the database.");
    throw;
}

#endregion


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) app.MapOpenApi();

app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

app.UseHttpsRedirection();

app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapLogWorkoutEndpoint();
app.MapViewUserProgressEndpoint();
app.MapLogWeightEndpoint();

app.Run();