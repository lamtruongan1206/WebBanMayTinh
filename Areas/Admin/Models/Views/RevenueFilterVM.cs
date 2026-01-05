namespace WebBanMayTinh.Areas.Admin.Models.Views
{
    public class RevenueFilterVM
    {
        public int Month { get; set; }
        public int Year { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TotalOrders { get; set; }
        public List<decimal> DailyRevenue { get; set; } = new();
        public List<string> DailyLabels { get; set; } = new();
    }
}
