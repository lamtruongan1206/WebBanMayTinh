using Microsoft.AspNetCore.Authorization;

namespace WebBanMayTinh.Authorization
{
    public sealed class HasPermissionAttribute : AuthorizeAttribute
    {
        public HasPermissionAttribute() { }
        public HasPermissionAttribute(string permissionType, string permissionValue) : base(policy: $"{permissionType}:{permissionValue}")
        {
            
        }
    }
}
