using System.ComponentModel.DataAnnotations;

namespace WebBanMayTinh.Areas.Admin.Models.Views
{
    public class RoleCreateVM
    {
        [Required]
        public string Name { get; set; }

        public List<PermissionVM> Permissions { get; set; } = [];
    }
}
