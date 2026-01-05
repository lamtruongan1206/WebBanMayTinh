namespace WebBanMayTinh.Models.Views
{
    public class OrderVM
    {
        public Guid Id { get; set; }
        public decimal TotalAmount { get; set; }
        public int Quantity { get; set; }
        public List<OrderItemVM> Items { get; set; }
        public OrderStatus OrderStatus { get; set; }
        public DateTime CreateAt { get; set; }  
        public bool IsReviewed { get; set; }
        public bool IsCancelRequested { get; set; } = false;
    }

    public class OrderDetailVM : Order
    {
        public decimal TotalAmount { get; set; }
        public int Quantity { get; set; }
        public List<OrderItemVM> Items { get; set; }
    }
}
