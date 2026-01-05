using WebBanMayTinh.Models;

namespace WebBanMayTinh.Areas.Admin.Models.Views
{
    public class DashboardVM
    {
        // KPI
        public int TotalOrders { get; set; }
        public int TotalProducts { get; set; }
        public int TotalUsers { get; set; }

        public decimal RevenueThisMonth { get; set; }
        public decimal RevenueLastMonth { get; set; }
        public decimal RevenueGrowthPercent { get; set; }

        // Chart
        public List<string> MonthlyLabels { get; set; } = new();
        public List<decimal> MonthlyRevenue { get; set; } = new();

        public List<string> CategoryNames { get; set; } = new();
        public List<int> ProductCountByCategory { get; set; } = new();

        // Tables
        public List<Order> RecentOrders { get; set; } = new();
        public List<Product> LowStockProducts { get; set; } = new();
    }
}
