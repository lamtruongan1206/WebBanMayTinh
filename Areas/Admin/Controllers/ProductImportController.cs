using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebBanMayTinh.Models;
using WebBanMayTinh.Authorization;

namespace WebBanMayTinh.Areas.Admin.Controllers
{
    [Area("Admin")]
    [HasPermission(CustomClaimTypes.Permission, Permissions.ProductImportAccess)]
    public class ProductImportController : Controller
    {
        private readonly DataContext _context;

        public ProductImportController(DataContext context)
        {
            _context = context;
        }

        // XEM LỊCH SỬ NHẬP HÀNG
        [HasPermission(CustomClaimTypes.Permission, Permissions.ProductImportRead)]
        public IActionResult Index()
        {
            var imports = _context.ProductImports
                .Include(x => x.Product)
                .OrderByDescending(x => x.ImportDate)
                .ToList();

            return View(imports);
        }
    }
}
