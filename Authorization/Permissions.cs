using System.Reflection;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;

namespace WebBanMayTinh.Authorization
{
    public static class Permissions
    {
        public const string AdminView = "admin:view";

        public const string UserAccess = "user:access";
        public const string UserCreate = "user:create";
        public const string UserUpdate = "user:update";
        public const string UserDelete = "user:delete";
        public const string UserPermission = "user:permission";
        public const string UserRead = "user:read";

        public const string ProductAccess = "product:access";
        public const string ProductCreate = "product:create";
        public const string ProductUpdate = "product:update";
        public const string ProductDelete = "product:delete";
        public const string ProductRead = "product:read";

        public const string BrandAccess = "brand:access";
        public const string BrandCreate = "brand:create";
        public const string BrandUpdate = "brand:update";
        public const string BrandDelete = "brand:delete";
        public const string BrandRead = "brand:read";

        public const string OrderAccess = "order:access";
        public const string OrderCreate = "order:create";
        public const string OrderUpdate = "order:update";
        public const string OrderDelete = "order:delete";
        public const string OrderRead = "order:read";

        public const string InvoiceAccess = "invoice:access";
        public const string InvoiceCreate = "invoice:create";
        public const string InvoiceUpdate = "invoice:update";
        public const string InvoiceDelete = "invoice:delete";
        public const string InvoiceRead = "invoice:read";

        public const string SliderAccess = "slider:access";
        public const string SliderCreate = "slider:create";
        public const string SliderUpdate = "slider:update";
        public const string SliderDelete = "slider:delete";
        public const string SliderRead = "slider:read";

        public const string CategoryAccess = "category:access";
        public const string CategoryCreate = "category:create";
        public const string CategoryUpdate = "category:update";
        public const string CategoryDelete = "category:delete";
        public const string CategoryRead = "category:read";

        public const string ProductImportAccess = "product_import:access";
        public const string ProductImportCreate = "product_import:create";
        public const string ProductImportUpdate = "product_import:update";
        public const string ProductImportDelete = "product_import:delete";
        public const string ProductImportRead = "product_import:read";

        public const string RoleAccess = "role:access";
        public const string RoleCreate = "role:create";
        public const string RoleUpdate = "role:update";
        public const string RoleDelete = "role:delete";
        public const string RoleRead = "role:read";

        public const string DashboardAccess = "dashboard:access";

        public static async Task SeedPermissionsAsync(this WebApplication app)
        {
            var scope = app.Services.CreateScope();

            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            const string adminRole = "Admin";
            var role = await roleManager.FindByNameAsync(adminRole);

            if (role == null)
            {
                role = new IdentityRole(adminRole);
                await roleManager.CreateAsync(role);
            }

            var allPermission = typeof(Permissions)
                .GetFields()
                .Select(f => f.GetValue(null)?.ToString())
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .ToList();

            var existingClaims = await roleManager.GetClaimsAsync(role);
            var existingPermissions = existingClaims
                .Where(c => c.Type == CustomClaimTypes.Permission)
                .Select(c => c.Value)
                .ToHashSet();

            foreach (var permission in allPermission)
            {
                if (!existingPermissions.Contains(permission))
                {
                    await roleManager.AddClaimAsync(
                        role,
                        new Claim(CustomClaimTypes.Permission, permission)
                    );
                }
            }
        }

        public static List<string> GetAll()
        {
            return typeof(Permissions)
                .GetFields(BindingFlags.Public | BindingFlags.Static)
                .Select(f => f.GetValue(null)!.ToString()!)
                .ToList();
        }
    }
}
