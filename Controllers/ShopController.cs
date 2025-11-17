using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebBanMayTinh.Models;

namespace WebBanMayTinh.Controllers
{
    public class ShopController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        ShopBanMayTinhContext conn;
        public ShopController(ILogger<HomeController> logger, ShopBanMayTinhContext conn)
        {
            _logger = logger;
            this.conn = conn;
        }


        public IActionResult Index()
        {
            var computers = conn.Computers
                .Include(c => c.Categories)
                .Include(c => c.Images)
                .ToList();
            return View(computers);
        }
    }
}
