using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebBanMayTinh.Authorization;
using WebBanMayTinh.Models;

namespace WebBanMayTinh.Controllers
{

    [Area("Admin")]
    [HasPermission(CustomClaimTypes.Permission, Permissions.CategoryAccess)]
    public class CategoryController : Controller
    {
        private readonly DataContext _context;

        public CategoryController(DataContext context)
        {
            _context = context;
        }

        // ================= INDEX =================
        [HasPermission(CustomClaimTypes.Permission, Permissions.CategoryRead)]
        public IActionResult Index()
        {
            var categories = _context.Categories
                .AsNoTracking()
                .OrderBy(c => c.Name)
                .ToList();

            return View(categories);
        }

        // ================= DETAILS =================
        [HasPermission(CustomClaimTypes.Permission, Permissions.CategoryRead)]
        public IActionResult Details(Guid id)
        {
            var category = _context.Categories
                .AsNoTracking()
                .FirstOrDefault(c => c.Id == id);

            if (category == null)
                return NotFound();

            return View(category);
        }

        // ================= CREATE =================
        [HasPermission(CustomClaimTypes.Permission, Permissions.CategoryCreate)]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [HasPermission(CustomClaimTypes.Permission, Permissions.CategoryCreate)]
        public IActionResult Create(Category category)
        {
            if (!ModelState.IsValid)
                return View(category);

            category.Id = Guid.NewGuid();
            category.CreateAt = DateOnly.FromDateTime(DateTime.Now);

            _context.Categories.Add(category);
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        // ================= EDIT =================
        [HasPermission(CustomClaimTypes.Permission, Permissions.CategoryUpdate)]
        public IActionResult Edit(Guid id)
        {
            var category = _context.Categories.Find(id);

            if (category == null)
                return NotFound();

            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [HasPermission(CustomClaimTypes.Permission, Permissions.CategoryUpdate)]
        public IActionResult Edit(Guid id, Category category)
        {
            if (id != category.Id)
                return BadRequest();

            if (!ModelState.IsValid)
                return View(category);

            _context.Update(category);
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        // ================= DELETE =================
        [HasPermission(CustomClaimTypes.Permission, Permissions.CategoryDelete)]
        public IActionResult Delete(Guid id)
        {
            var category = _context.Categories
                .AsNoTracking()
                .FirstOrDefault(c => c.Id == id);

            if (category == null)
                return NotFound();

            return View(category);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [HasPermission(CustomClaimTypes.Permission, Permissions.CategoryDelete)]
        public IActionResult DeleteConfirmed(Guid id)
        {
            var category = _context.Categories
       .Include(c => c.Products)
       .FirstOrDefault(c => c.Id == id);

            if (category == null) return NotFound();

            // Set CategoryId của các product liên quan về null
            foreach (var product in category.Products)
            {
                product.CategoryId = null;
            }

            _context.Categories.Remove(category);
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }
    }
}
