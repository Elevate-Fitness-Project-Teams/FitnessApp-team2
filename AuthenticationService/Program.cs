using AuthenticationService.Common.Behaviors;
using AuthenticationService.Data;
using AuthenticationService.Data.Entities;
using AuthenticationService.Middleware;
using AuthenticationService.Options;
using AuthenticationService.Services;
using FluentValidation;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();

builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
    cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
});

builder.Services.AddIdentity<User, IdentityRole>(options =>
    {
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = true;
        options.Password.RequiredLength = 8;

        options.User.RequireUniqueEmail = true;
        options.SignIn.RequireConfirmedEmail = true;

        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
        options.Lockout.MaxFailedAccessAttempts = 5;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddMassTransit(x =>
{
    x.SetKebabCaseEndpointNameFormatter();

    x.AddEntityFrameworkOutbox<ApplicationDbContext>(o =>
    {
        o.UseSqlServer();
        o.UseBusOutbox();
    });

    x.UsingRabbitMq((context, cfg) =>
    {
        var rabbitMqConfig = builder.Configuration.GetSection("RabbitMq");
        var host = rabbitMqConfig["Host"] ?? "rabbitmq";
        var virtualHost = rabbitMqConfig["VirtualHost"] ?? "/";
        var username = rabbitMqConfig["Username"] ?? "guest";
        var password = rabbitMqConfig["Password"] ?? "guest";

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


builder.Services.AddOptions<JwtOptions>()
    .BindConfiguration(JwtOptions.SectionName)
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddScoped<IJwtProvider, JwtProvider>();
var jwtSettings = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>();

builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(o =>
    {
        o.SaveToken = true;
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            IssuerSigningKey = RsaKeyService.GetPublicKey(jwtSettings!.PublicKeyPath, jwtSettings.KeyId),
            ValidIssuer = jwtSettings?.Issuer,
            ValidAudience = jwtSettings?.Audience
        };
    });

builder.Services.AddAuthorization();


builder.Services.AddScoped<IGrpcIntegrationService, GrpcIntegrationService>();
builder.Services.AddHttpContextAccessor();

builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();

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

// Global exception handler (must be first in the pipeline)
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

app.UseSwagger();
app.UseSwaggerUI();
app.MapOpenApi();
app.MapHealthChecks("/health");

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();