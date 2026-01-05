using Microsoft.AspNetCore.Identity;
using WebBanMayTinh.Models;

namespace WebBanMayTinh.Utils
{
    public static class DataSeed
    {
        public static async Task SeedAdminAsync(this WebApplication app)
        {
            using var scope = app.Services.CreateScope();

            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();

            const string adminRole = "Admin";

            // 1. Tạo role Admin nếu chưa có
            if (!await roleManager.RoleExistsAsync(adminRole))
            {
                await roleManager.CreateAsync(new IdentityRole(adminRole));
            }

            // 2. Tạo tài khoản Admin nếu chưa có
            var adminEmail = "admin@gmail.com";
            var username = "admin";
            var adminUser = await userManager.FindByNameAsync(username);

            if (adminUser == null)
            {
                adminUser = new AppUser
                {
                    UserName = "admin",
                    Email = adminEmail,
                    EmailConfirmed = true,
                    FirstName = "System",
                    LastName = "Admin",
                };

                var result = await userManager.CreateAsync(adminUser, "Admin@123");

                if (!result.Succeeded)
                    throw new Exception("Không tạo được tài khoản Admin");
            }

            // 3. Gán role Admin
            if (!await userManager.IsInRoleAsync(adminUser, adminRole))
            {
                await userManager.AddToRoleAsync(adminUser, adminRole);
            }
        }
    }
}
