using FluentValidation;
using Microsoft.EntityFrameworkCore;
using WorkoutService.Data;
using WorkoutService.Middleware;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped(typeof(IGeneralRepo<>), typeof(GeneralRepo<>));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);
builder.Services.AddMediatR(cfg => 
{
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
    cfg.AddOpenBehavior(typeof(WorkoutService.Common.Behaviors.ValidationBehavior<,>));
});

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

// Seed baseline exercises and workout plans for development and testing
await WorkoutDbSeeder.SeedAsync(app.Services);

#endregion

app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

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

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();


