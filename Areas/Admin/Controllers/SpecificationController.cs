using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebBanMayTinh.Models;
using WebBanMayTinh.Authorization;

namespace WebBanMayTinh.Areas.Admin.Controllers
{
    [Area("Admin")]
    [HasPermission(CustomClaimTypes.Permission, Permissions.ProductUpdate)]
    public class SpecificationController : Controller
    {
        private readonly DataContext _context;

        public SpecificationController(DataContext context)
        {
            _context = context;
        }

        // ================== LIST ==================
        public IActionResult Index(Guid productId)
        {
            var product = _context.Products
                .Include(p => p.Specifications)
                .FirstOrDefault(p => p.Id == productId);

            if (product == null) return NotFound();

            ViewBag.ProductName = product.Name;
            ViewBag.ProductId = productId;

            return View(product.Specifications.ToList());
        }

        // ================== CREATE ==================
        [HttpGet]
        public IActionResult Create(Guid productId)
        {
            return View(new Specification { ProductId = productId });
        }

        [HttpPost]
        public IActionResult Create(Specification model)
        {
            if (!ModelState.IsValid) return View(model);

            model.Id = Guid.NewGuid();
            _context.Specifications.Add(model);
            _context.SaveChanges();

            return RedirectToAction("Index", new { productId = model.ProductId });
        }

        // ================== EDIT ==================
        [HttpGet]
        public IActionResult Edit(Guid id)
        {
            var spec = _context.Specifications.Find(id);
            if (spec == null) return NotFound();
            return View(spec);
        }

        [HttpPost]
        public IActionResult Edit(Specification model)
        {
            if (!ModelState.IsValid) return View(model);

            _context.Specifications.Update(model);
            _context.SaveChanges();

            return RedirectToAction("Index", new { productId = model.ProductId });
        }

        // ================== DELETE ==================
        [HttpPost]
        public IActionResult Delete(Guid id)
        {
            var spec = _context.Specifications.Find(id);
            if (spec == null) return NotFound();

            var productId = spec.ProductId;
            _context.Specifications.Remove(spec);
            _context.SaveChanges();

            return RedirectToAction("Index", new { productId });
        }
    }
}
