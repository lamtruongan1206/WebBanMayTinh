using System.ComponentModel.DataAnnotations;

namespace WebBanMayTinh.Models
{
    public class Address
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required, Display(Name = "Tên người nhận")]
        public string ReceiverName { get; set; }

        [Required, Display(Name = "Số điện thoại")]
        public string Phone { get; set; }

        [Required, Display(Name = "Đường, Số nhà ...")]
        public string AddressLine { get; set; }

        [Required, Display(Name = "Xã")]
        public string Ward { get; set; }
        [Required, Display(Name = "Huyện")]
        public string District { get; set; }
        [Required, Display(Name = "Tỉnh")]
        public string Province { get; set; }

        [Required, Display(Name = "Đặt làm địa chỉ giao hàng")]
        public bool IsDefault { get; set; } = true;

        public bool IsActive { get; set; } = true;

        public string? UserId { get; set; }
        public AppUser? User { get; set; }
    }
}
