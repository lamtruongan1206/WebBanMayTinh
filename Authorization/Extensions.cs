using System.Data;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
namespace WebBanMayTinh.Authorization
{
    public static class Extensions
    {
        public static async Task SeedRolesPermissionsAsync(this WebApplication app)
        {
            var scope = app.Services.CreateScope();

            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();


            var adminRole = await roleManager.FindByNameAsync("Admin");

            if (adminRole is null)
            {
                await roleManager.CreateAsync(adminRole = new IdentityRole("Admin"));

                await roleManager.AddClaimAsync(adminRole, new Claim(CustomClaimTypes.Permission, Permissions.UserAccess));
                await roleManager.AddClaimAsync(adminRole, new Claim(CustomClaimTypes.Permission, Permissions.UserRead));
                await roleManager.AddClaimAsync(adminRole, new Claim(CustomClaimTypes.Permission, Permissions.UserUpdate));
                await roleManager.AddClaimAsync(adminRole, new Claim(CustomClaimTypes.Permission, Permissions.UserDelete));
                await roleManager.AddClaimAsync(adminRole, new Claim(CustomClaimTypes.Permission, Permissions.UserCreate));
            }

            var memberRole = await roleManager.FindByNameAsync("Member");
            if (memberRole is null)
            {
                await roleManager.CreateAsync(memberRole = new IdentityRole("Member"));
                await roleManager.AddClaimAsync(memberRole, new Claim(CustomClaimTypes.Permission, Permissions.UserRead));
                await roleManager.AddClaimAsync(memberRole, new Claim(CustomClaimTypes.Permission, Permissions.UserUpdate));
            }
        }
    }
}
