using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using UserProfileService.Common;
using UserProfileService.Common.Behaviors;
using UserProfileService.Common.Database;
using UserProfileService.Features.Profiles.ChangePassword;
using UserProfileService.Features.Profiles.GetProfile;
using UserProfileService.Features.Profiles.UpdateProfile;
using UserProfileService.Features.Profiles.UploadProfilePicture;
using UserProfileService.Features.Settings.GetSettings;
using UserProfileService.Features.Settings.UpdateSettings;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

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
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Bind the "ServiceUrls" section from appsettings.json to the ServiceUrls class
builder.Services.Configure<ServiceUrls>(builder.Configuration.GetSection(ServiceUrls.SectionName));

// Register a typed HttpClient for ChangePasswordHandler (used to call Auth Service)
builder.Services.AddHttpClient<ChangePasswordHandler>();

var jwtSecret = builder.Configuration["Jwt:Secret"] ?? "super-secret-key-that-is-at-least-32-bytes-long";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "FitnessApp",
            ValidAudience = builder.Configuration["Jwt:Audience"] ?? "FitnessAppUsers",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// Automatically apply any pending EF Core migrations on startup
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await dbContext.Database.MigrateAsync();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Serve uploaded files (profile pictures, etc.) from the wwwroot/ folder
app.UseStaticFiles();
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

// Map minimal API endpoints
app.MapGetProfileEndpoint();
app.MapUpdateProfileEndpoint();
app.MapUploadProfilePictureEndpoint();
app.MapChangePasswordEndpoint();
app.MapGetSettingsEndpoint();
app.MapUpdateSettingsEndpoint();

app.Run();
