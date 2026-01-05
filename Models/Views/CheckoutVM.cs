using WebBanMayTinh.Models.Enums;
namespace WebBanMayTinh.Models.Views

{
    public class CheckoutVM
    {
        public List<ProductCheckoutVM> Products { get; set; } = [];
        public Address? Address { get; set; }
        public decimal TotalAmount { get; set; } = 0;
        public decimal ShippingFee { get; set; } = 0;
        public decimal Subtotal { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
    }
}
