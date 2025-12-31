using System.ComponentModel.DataAnnotations;

namespace WebBanMayTinh.Models.Views
{
    public class ChangePasswordVM
    {
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }

        [Compare("NewPassword", ErrorMessage = "Mật khẩu không khớp")]
        public string ConfirmPassword { get; set; }
        public string Username { get; set; }    
    }
}
