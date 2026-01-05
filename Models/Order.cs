using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebBanMayTinh.Models
{
    public enum OrderStatus
    {
        Pending = 0,
        Confirmed = 1,
        Shipping = 2,
        Completed = 3,
        Cancelled = 4
    }


    public class Order
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        [Display(Name = "Thời gian tạo")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        [Display(Name = "Thời gian cập nhật")]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
        [Display(Name = "Trạng thái")]
        public OrderStatus Status { get; set; } = OrderStatus.Pending;
        [Display(Name = "Tiền hàng")]
        public decimal Subtotal { get; set; }
        [Display(Name = "Tiền ship")]
        public decimal ShippingFee { get; set; }
        [Display(Name = "Tổng")]
        public decimal TotalAmount { get; set; }
        [Display(Name = "Số lượng")]
        public int Quantity { get; set; }
        [Display(Name = "Khách hàng đã nhận")]
        public bool IsReceived { get; set; } = false;
        [Display(Name = "Thời gian nhận hàng")]
        public DateTime? ReceivedTime { get; set; }

        [Display(Name = "Kiểu thanh toán")]
        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.CASH_ON_DELIVERY;
        public bool IsReviewed { get; set; }

        // Người dùng
        [Required]
        public string UserId { get; set; }
        [ForeignKey("UserId"), Display(Name = "Tài khoản")]
        public virtual AppUser User { get; set; }


        // Địa chỉ
        [Required]
        public Guid AddressId { get; set; }
        [ForeignKey("AddressId"), Display(Name = "Địa chỉ")]
        public Address Address { get; set; }





        // Chức năng yêu cầu hủy đơn hàng
        [Display(Name = "Yêu cầu hủy")]
        public bool IsCancelRequested { get; set; } = false;

        [Display(Name = "Lý do hủy")]
        public string? CancelReason { get; set; }

        [Display(Name = "Thời gian yêu cầu hủy")]
        public DateTime? CancelRequestedAt { get; set; }

        [Display(Name = "Thời gian hủy")]
        public DateTime? CancelledAt { get; set; }

        public List<OrderItems> OrderItems { get; set; }
        public List<OrderStatusHistory> OrderStatusHistories { get; set; } = new List<OrderStatusHistory>();
        
    }
}
