using FluentValidation;
using FluentValidation.AspNetCore;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using UserProfileService.Common;
using UserProfileService.Common.Behaviors;
using UserProfileService.Common.Database;
using UserProfileService.Common.DataBase;
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
builder.Services.AddFluentValidationAutoValidation()
    .AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
builder.Services.AddHttpClient();


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

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

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
