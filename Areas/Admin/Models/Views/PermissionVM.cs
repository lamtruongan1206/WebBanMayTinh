using Microsoft.AspNetCore.Identity;

namespace WebBanMayTinh.Areas.Admin.Models.Views
{
    public class PermissionVM
    {
        public IdentityRole Role { get; set; }
        public bool Checked { get; set; }
    }
}
