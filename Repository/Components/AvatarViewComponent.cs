using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using WebBanMayTinh.Models.Views;

namespace WebBanMayTinh.Repository.Components
{
    public class AvatarViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            if (UserClaimsPrincipal == null || UserClaimsPrincipal.Identity == null)
                return View(new HeaderUserVM());
            if (!UserClaimsPrincipal.Identity.IsAuthenticated)
                return View(new HeaderUserVM());

            return View(new HeaderUserVM
            {
                UserName = UserClaimsPrincipal.Identity.Name ?? "",
                Avatar = UserClaimsPrincipal.FindFirst("Avatar")?.Value ?? "",
                FullName = UserClaimsPrincipal.FindFirst("FullName")?.Value ?? "",
                Role = UserClaimsPrincipal.FindFirst(ClaimTypes.Role)?.Value ?? ""
            });
        }
    }
}
