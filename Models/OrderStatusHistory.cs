namespace WebBanMayTinh.Models
{
    public class OrderStatusHistory
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateTime UpdateTime { get; set; }    
        public OrderStatus OrderStatus { get; set; }
        public Guid OrderId { get; set; }
        public Order Order { get; set; }
    }
}
