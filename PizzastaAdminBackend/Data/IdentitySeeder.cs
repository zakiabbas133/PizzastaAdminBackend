using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PizzastaAdminBackend.Models;

namespace PizzastaAdminBackend.Data
{
    public static class IdentitySeeder
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            // Make sure migrations are applied
            await context.Database.MigrateAsync();

            const string username = "pastizzaadmin";
            const string email = "pastizzaadmin@pastizza.com";
            const string password = "Pastizzaadmin@123";

            // Check if user already exists
            var existingUser = await userManager.FindByNameAsync(username);

            if (existingUser != null)
            {
                return;
            }

            // Create user
            var user = new ApplicationUser
            {
                UserName = username,
                Email = email,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(user, password);

            if (!result.Succeeded)
            {
                var errors = string.Join(
                    ", ",
                    result.Errors.Select(x => $"{x.Code}: {x.Description}")
                );

                throw new Exception(
                    $"Failed to create default admin user: {errors}"
                );
            }
        }
    }
}