using Microsoft.AspNetCore.Identity;
using CRM.Models;

namespace CRM.Data.seed

{
    public class IdentitySeed
    {

        public static async Task SeedAsync(IServiceProvider services)
        {
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

            string[] roleNames = { "Admin", "SalesManager", "SalesExecutive" };

            // 2. Loop through them and create if they don't exist
            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // 3. Define Admin User Details
            var adminEmail = "admin@crm.com"; // Use a professional domain if possible
            var adminName = "System Administrator";

            var existingAdmin = await userManager.FindByEmailAsync(adminEmail);

            if (existingAdmin == null)
            {
                var adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FullName = adminName,
                    EmailConfirmed = true,
                };

                // 4. Create the user
                var createResult = await userManager.CreateAsync(adminUser, "Admin@123");

                // 5. Assign the Admin role
                if (createResult.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                    Console.WriteLine("Admin user seeded successfully.");
                }
                else
                {
                    // Log errors if seeding fails (e.g. password too weak)
                    var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
                    Console.WriteLine($"Error seeding Admin user: {errors}");
                }
            }
        }

        }
        }
    }
