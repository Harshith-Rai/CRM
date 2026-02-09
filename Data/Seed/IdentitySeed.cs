using Microsoft.AspNetCore.Identity;
using CRM.Models;

namespace MovieReview.Data.seed
{
    public class identitySeed
    {

        public static async Task SeedAsync(IServiceProvider services)
        {
            var RoleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            var UserManager = services.GetRequiredService<UserManager<ApplicationUser>>();

            if (!await RoleManager.RoleExistsAsync("Admin"))
            {
                await RoleManager.CreateAsync(new IdentityRole("Admin"));
            }

            if (!await RoleManager.RoleExistsAsync("User"))
            {
                await RoleManager.CreateAsync(new IdentityRole("User"));
            }


            var adminEmail = "admin@gmail.com";
            var adminpassword = "Admin@123";
            var adminName = "admin";

            var existingAdmin = await UserManager.FindByEmailAsync(adminEmail);

            Console.WriteLine("execyuting is " + existingAdmin + " here how ");

            if (existingAdmin == null)
            {


                Console.WriteLine("admin is null");
                var admin = new ApplicationUser()
                {
                    Email = adminEmail,
                    EmailConfirmed = true,
                    UserName = adminEmail,
                    FullName = adminName,

                };


                var result = await UserManager.CreateAsync(admin, adminpassword);

                await UserManager.AddToRoleAsync(admin, "Admin");
            }


        }



    }
}
