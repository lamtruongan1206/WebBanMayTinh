using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebBanMayTinh.Models;

namespace WebBanMayTinh.Controllers
{

    [Area("Admin")]
    public class CategoryController : Controller
    {
        private readonly DataContext _context;

        public CategoryController(DataContext context)
        {
            _context = context;
        }

        // ================= INDEX =================
        public IActionResult Index()
        {
            var categories = _context.Categories
                .AsNoTracking()
                .OrderBy(c => c.Name)
                .ToList();

            return View(categories);
        }

        // ================= DETAILS =================
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
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
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
        public IActionResult Edit(Guid id)
        {
            var category = _context.Categories.Find(id);

            if (category == null)
                return NotFound();

            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
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
        public IActionResult DeleteConfirmed(Guid id)
        {
            var category = _context.Categories.Find(id);

            if (category == null)
                return NotFound();

            _context.Categories.Remove(category);
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }
    }
}
