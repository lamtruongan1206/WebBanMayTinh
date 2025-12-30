namespace WebBanMayTinh.Models.Views
{
    public class OrderVM
    {
        public decimal TotalAmount { get; set; }
        public int Quantity { get; set; }
        public List<OrderItemVM> Items { get; set; }
        public OrderStatus OrderStatus { get; set; }
    }
}
