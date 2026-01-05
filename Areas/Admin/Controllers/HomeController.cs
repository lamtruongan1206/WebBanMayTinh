using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebBanMayTinh.Areas.Admin.Models.Views;
using WebBanMayTinh.Authorization;
using WebBanMayTinh.Models;
using WebBanMayTinh.Models.DTO;

namespace WebBanMayTinh.Areas.Admin.Controllers
{
    [Area("Admin")]
    [HasPermission(CustomClaimTypes.Permission, Permissions.AdminView)]
    public class HomeController : Controller
    {
        DataContext _context;

        public HomeController(DataContext conn)
        {
            this._context = conn;
        }
        public async Task<IActionResult> Index()
        {
            var now = DateTime.Now;

            // 6 tháng gần nhất
            var lastSixMonths = Enumerable.Range(0, 6)
                .Select(i => new DateTime(now.Year, now.Month, 1).AddMonths(-i))
                .OrderBy(d => d)
                .ToList();

            // Doanh thu theo tháng (1 QUERY)
            var revenueByMonth = await _context.Orders
                .GroupBy(o => new { o.CreatedAt.Year, o.CreatedAt.Month })
                .Select(g => new
                {
                    g.Key.Year,
                    g.Key.Month,
                    Total = g.Sum(x => x.TotalAmount)
                }).ToListAsync();

            // Doanh thu tháng này / tháng trước
            var thisMonthRevenue = revenueByMonth
                .FirstOrDefault(x => x.Year == now.Year && x.Month == now.Month)?.Total ?? 0;

            var lastMonth = now.AddMonths(-1);
            var lastMonthRevenue = revenueByMonth
                .FirstOrDefault(x => x.Year == lastMonth.Year && x.Month == lastMonth.Month)?.Total ?? 0;

            decimal growth = lastMonthRevenue == 0
                ? 100
                : (thisMonthRevenue - lastMonthRevenue) / lastMonthRevenue * 100;

            var dashboard = new DashboardVM
            {
                TotalOrders = await _context.Orders.CountAsync(),
                TotalProducts = await _context.Products.CountAsync(),
                TotalUsers = await _context.Users.CountAsync(),

                RevenueThisMonth = thisMonthRevenue,
                RevenueLastMonth = lastMonthRevenue,
                RevenueGrowthPercent = Math.Round(growth, 2),

                MonthlyLabels = lastSixMonths.Select(m => m.ToString("MM/yyyy")).ToList(),
                MonthlyRevenue = lastSixMonths.Select(m =>
                    revenueByMonth.FirstOrDefault(x => x.Year == m.Year && x.Month == m.Month)?.Total ?? 0
                ).ToList(),

                CategoryNames = await _context.Categories.Select(c => c.Name).ToListAsync(),
                ProductCountByCategory = await _context.Categories
                    .Select(c => c.Products.Count()).ToListAsync(),

                RecentOrders = await _context.Orders
                    .Include(o => o.User)
                    .OrderByDescending(o => o.CreatedAt)
                    .Take(5)
                    .ToListAsync(),

                LowStockProducts = await _context.Products
                    .Where(p => p.Quantity < 10)
                    .OrderBy(p => p.Quantity)
                    .ToListAsync()
            };

            return View(dashboard);
        }
    }
}
