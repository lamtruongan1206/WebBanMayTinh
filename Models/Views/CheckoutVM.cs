namespace WebBanMayTinh.Models.Views
{
    public class CheckoutVM
    {
        public List<Product> Products { get; set; } = [];
        public decimal TotalAmount { get; set; } = 0;
        public decimal ShippingFee { get; set; } = 0;
    }
}
