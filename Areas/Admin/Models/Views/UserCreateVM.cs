using System.ComponentModel.DataAnnotations;

namespace WebBanMayTinh.Areas.Admin.Models.Views
{
    public class UserCreateVM
    {
        [Display(Name = "Tài khoản"), MinLength(4, ErrorMessage = "Tài khoản ít nhất 4 ký tự") , Required(ErrorMessage = "Tài khoản không được để trống")]
        public string Username { get; set; }

        [Display(Name = "Mật khẩu"), Required(ErrorMessage = "Mật khẩu không được để trống"), MinLength(8, ErrorMessage = "Mật khẩu ít nhất 8 ký tự")]
        public string Password { get; set; }

        [Display(Name = "Nhập lại mật khẩu"), Compare("Password", ErrorMessage = "Mật khẩu không trùng khớp")]
        public string ConfirmPassword { get; set; }
        
        [Display(Name = "Họ")]
        public string? FirstName { get; set; }
        
        [Display(Name = "Tên")]
        public string? LastName { get; set; }

        [Display(Name = "Email"), Required(ErrorMessage = "Email không được để trống"), EmailAddress(ErrorMessage = "Không đúng định dạng Email")]
        public string Email { get; set; }
        
        [Display(Name = "Số điện thoại"), Required(ErrorMessage = "Số điện thoại không được để trống")]
        public string Phone { get; set; }
        [Display(Name = "Địa chỉ"), Required(ErrorMessage = "Địa chỉ không được để trống")]
        public string Address { get; set; }
        [Display(Name = "Vai trò"), Required(ErrorMessage = "Vai trò không được để trống")]
        public Guid? RoleId { get; set; }
    }
}
