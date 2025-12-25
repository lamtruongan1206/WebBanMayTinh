
using System.ComponentModel.DataAnnotations;

namespace WebBanMayTinh.Models.Views
{
    public class UserLoginVM
    {
        [Required(ErrorMessage = "Tài khoản không được để trống")]
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
