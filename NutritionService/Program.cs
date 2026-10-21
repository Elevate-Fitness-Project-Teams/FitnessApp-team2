using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NutritionService.Common;
using NutritionService.Common.Behaviors;
using NutritionService.Common.Database;
using NutritionService.Middleware;
using NutritionService.Services;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
    options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
});

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

// Register all FluentValidation validators in the assembly
builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);

// Register the pipeline behavior so MediatR calls it before handlers
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
    cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
});

// Register HttpClient for FCE service and the HTTP client wrapper
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient("FitnessCalculationService", client =>
{
    var baseUrl = builder.Configuration["ServiceUrls:FitnessCalculationService"]
                  ?? "http://fitnesscalculationservice:8080";
    client.BaseAddress = new Uri(baseUrl);
});
builder.Services.AddScoped<IFceHttpClient, FceHttpClient>();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
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

// Bind the "ServiceUrls" section from appsettings.json to the ServiceUrls class
builder.Services.Configure<ServiceUrls>(builder.Configuration.GetSection(ServiceUrls.SectionName));

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

var app = builder.Build();

// Automatically apply any pending EF Core migrations on startup
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
}

// Seed baseline nutrition recipes and meal plans for development and testing
await NutritionDbSeeder.SeedAsync(app.Services);

#endregion

// Configure the HTTP request pipeline.
app.MapOpenApi();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/openapi/v1.json", "v1");
    options.RoutePrefix = "swagger";
});
app.MapGet("/", () => Results.Redirect("/swagger"));

app.MapHealthChecks("/health");

app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
