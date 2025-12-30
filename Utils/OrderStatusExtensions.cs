using WebBanMayTinh.Models;

namespace WebBanMayTinh.Utils
{
    public static class OrderStatusExtensions
    {
        public static string ToTextColor(this OrderStatus status)
        {
            return status switch
            {
                OrderStatus.Pending => "text-warning",
                OrderStatus.Confirmed => "text-primary",
                OrderStatus.Shipping => "text-info",
                OrderStatus.Completed => "text-success",
                OrderStatus.Cancelled => "text-danger",
                _ => "text-secondary"
            };
        }

        public static string ToVietNamText(this OrderStatus status)
        {
            return status switch
            {
                OrderStatus.Pending => "Đang chờ",
                OrderStatus.Confirmed => "Đã xác nhận",
                OrderStatus.Shipping => "Đang giao hàng",
                OrderStatus.Completed => "Đã giao",
                OrderStatus.Cancelled => "Đã hủy",
                _ => "Đang chờ"
            };
        }
    }
}
