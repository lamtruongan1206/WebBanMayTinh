using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace WebBanMayTinh.Models;

public partial class User
{
    public Guid Id { get; set; }

    [Display(Name = "Tên tài khoản")]
    public string? Username { get; set; }
    [Display(Name = "Mật khẩu")]
    public string? Password { get; set; }
    [Display(Name = "Họ")]

    public string? FirstName { get; set; }
    [Display(Name = "Tên")]
    public string? LastName { get; set; }

    [Display(Name = "Email")]
    public string? Email { get; set; }

    [Display(Name = "Số điện thoại")]
    public string? Phone { get; set; }

    [Display(Name = "Địa chỉ")]
    public string? Address { get; set; }

    [Display(Name = "Vai trò")]
    public Guid? RoleId { get; set; }

    [Display(Name = "Ngày tạo")]
    public DateOnly? CreatedAt { get; set; }

    [Display(Name = "Ngày cập nhật")]
    public DateOnly? UpdateAt { get; set; }

    public virtual ICollection<Bill> Bills { get; set; } = new List<Bill>();

    public virtual ICollection<Cart> Carts { get; set; } = new List<Cart>();

    public virtual Role? Role { get; set; }
}
