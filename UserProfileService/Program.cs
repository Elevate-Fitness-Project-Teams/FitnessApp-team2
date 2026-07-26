using FluentValidation;
using FluentValidation.AspNetCore;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Reflection;
using System.Security.Cryptography;
using UserProfileService.Common;
using UserProfileService.Common.Behaviors;
using UserProfileService.Common.Database;
using UserProfileService.Common.DataBase;
using UserProfileService.Features.Profiles.GetProfile;
using UserProfileService.Features.Profiles.UpdateProfile;
using UserProfileService.Features.Profiles.UploadProfilePicture;
using UserProfileService.Features.Settings.GetSettings;
using UserProfileService.Features.Settings.UpdateSettings;
using UserProfileService.MessageBroker;
using UserProfileService.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddFluentValidationAutoValidation()
    .AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
builder.Services.AddHttpClient();


// Register the pipeline behavior so MediatR calls it before handlers
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
    cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
});
// Add MassTransit Messaging
builder.Services.AddRabbitMqMessaging(builder.Configuration);

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
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

// Bind the "ServiceUrls" section from appsettings.json to the ServiceUrls class
builder.Services.Configure<ServiceUrls>(builder.Configuration.GetSection(ServiceUrls.SectionName));

var app = builder.Build();

// Automatically apply any pending EF Core migrations on startup
using (var scope = app.Services.CreateScope())
{
    try
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await dbContext.Database.MigrateAsync();
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while applying migrations.");
        throw; // Rethrow the exception to prevent the application from starting if migrations fail
    }
}

app.MapOpenApi();
app.MapHealthChecks("/health");

// Global exception handler — must be first to catch all exceptions
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

// Serve uploaded files (profile pictures, etc.) from the wwwroot/ folder
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();

// Map the endpoints for the profile features
app.MapGetProfileEndpoint();
app.MapUpdateProfileEndpoint();
app.MapUploadProfilePictureEndpoint();
app.MapGetSettingsEndpoint();
app.MapUpdateSettingsEndpoint();
app.Run();
