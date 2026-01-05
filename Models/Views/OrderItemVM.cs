namespace WebBanMayTinh.Models.Views
{
    public class OrderItemVM
    {
        public Guid ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductThumbnailUrl { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }
}
