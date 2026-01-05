using System.ComponentModel.DataAnnotations;

namespace WebBanMayTinh.Models
{
    public enum PaymentMethod
    {
        CASH_ON_DELIVERY = 0,
        ONLINE_PAYMENT = 1
    }

    public class Invoice
    {
        [Key]
        [Required]
        [Display(Name = "Mã hóa đơn")]
        public Guid Id { get; set; }

        [Required]
        [Display(Name = "Mã đơn hàng")]
        public Guid OrderId { get; set; }

        [Required]
        [Display(Name = "Thời gian tạo")]
        public DateTime CreateAt { get; set; }

        [Required]   
        public Order Order { get; set; }

        [Required]
        [Display(Name = "Phương thức thanh toán")]
        public PaymentMethod PaymentMethod { get; set; }
    }
}
