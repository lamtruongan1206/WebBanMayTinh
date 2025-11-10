using Microsoft.AspNetCore.Mvc;
using WebBanMayTinh.Models.Entities;
using WebBanMayTinh.Models;
using Microsoft.EntityFrameworkCore;

namespace WebBanMayTinh.Controllers
{
    public class UserController : Controller
    {
        ShopBanMayTinhContext conn = new ShopBanMayTinhContext();
        public IActionResult Index(string searchName, string searchManufacturer, decimal? priceFrom, decimal? priceTo, int page = 1, int pageSize = 6)
        {
            // Lấy dữ liệu cơ bản
            var query = conn.Computers
                .Include(c => c.Categories)
                .Include(c => c.Images)
                .AsQueryable();

            // Lọc theo tên
            if (!string.IsNullOrEmpty(searchName))
            {
                query = query.Where(c => c.Name.Contains(searchName));
            }

            // Lọc theo hãng sản xuất
            if (!string.IsNullOrEmpty(searchManufacturer))
            {
                query = query.Where(c => c.Manufacturer.Contains(searchManufacturer));
            }

            // Lọc theo khoảng giá
            if (priceFrom.HasValue)
                query = query.Where(c => c.Price >= priceFrom.Value);
            if (priceTo.HasValue)
                query = query.Where(c => c.Price <= priceTo.Value);

            // Tổng số bản ghi sau lọc
            int totalItems = query.Count();

            // Phân trang
            var computers = query
                .OrderBy(c => c.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // Truyền dữ liệu phân trang qua ViewBag
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            // Truyền lại các giá trị tìm kiếm để giữ trên form
            ViewBag.SearchName = searchName;
            ViewBag.SearchManufacturer = searchManufacturer;
            ViewBag.PriceFrom = priceFrom;
            ViewBag.PriceTo = priceTo;

            return View(computers);
        }

        [HttpGet]
        public IActionResult Detail(Guid id)
        {
            // Lấy máy tính theo id, bao gồm các ảnh
            var computer = conn.Computers
                .Include(c => c.Images)
                .Include(c => c.Categories) // Nếu muốn hiển thị tên danh mục
                .FirstOrDefault(c => c.Id == id);

            if (computer == null) return NotFound();

            // Truyền model vào view
            return View(computer);
        }
    }
}
