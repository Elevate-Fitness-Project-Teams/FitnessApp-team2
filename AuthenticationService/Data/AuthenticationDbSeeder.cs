using AuthenticationService.Data.Entities;
using Microsoft.AspNetCore.Identity;

namespace AuthenticationService.Data;

public static class AuthenticationDbSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<ApplicationDbContext>>();

        const string testUserId = "11111111-1111-1111-1111-111111111111";
        const string testEmail = "testuser@elevate.com";

        var existingUser = await userManager.FindByEmailAsync(testEmail);
        if (existingUser == null)
        {
            var user = new User
            {
                Id = testUserId,
                UserName = testEmail,
                Email = testEmail,
                FirstName = "Test",
                LastName = "User",
                PhoneNumber = "+1234567890",
                EmailConfirmed = true,
                PhoneNumberConfirmed = true,
                CreatedAt = DateTime.UtcNow,
                SecurityStamp = Guid.NewGuid().ToString()
            };

            var result = await userManager.CreateAsync(user, "Password123!");
            if (result.Succeeded)
            {
                logger.LogInformation("Successfully seeded default test user: {Email} (Id: {Id})", testEmail, testUserId);
            }
            else
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                logger.LogWarning("Failed to seed default test user: {Errors}", errors);
            }
        }
    }
}
