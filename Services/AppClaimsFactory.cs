using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using WebBanMayTinh.Models;

namespace WebBanMayTinh.Services
{
    public class AppClaimsFactory : UserClaimsPrincipalFactory<AppUser, IdentityRole>
    {
        public AppClaimsFactory(
        UserManager<AppUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IOptions<IdentityOptions> options)
        : base(userManager, roleManager, options)
        {
        }

        protected override async Task<ClaimsIdentity> GenerateClaimsAsync(AppUser user)
        {
            var identity = await base.GenerateClaimsAsync(user);

            identity.AddClaim(new Claim("Avatar", user.Avatar ?? ""));
            identity.AddClaim(new Claim("FullName", user.FirstName + " " + user.LastName ?? ""));
            return identity;
        }
    }
}
