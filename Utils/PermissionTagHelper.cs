using Microsoft.AspNetCore.Razor.TagHelpers;
using WebBanMayTinh.Authorization;

namespace WebBanMayTinh.Utils
{
    [HtmlTargetElement(Attributes = "permission")]
    public class PermissionTagHelper : TagHelper
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public PermissionTagHelper(IHttpContextAccessor accessor)
        {
            _httpContextAccessor = accessor;
        }

        public string Permission { get; set; } = null!;

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            var user = _httpContextAccessor.HttpContext?.User;

            if (user == null ||
                !user.HasClaim(CustomClaimTypes.Permission, Permission))
            {
                output.SuppressOutput();
            }
        }
    }
}
