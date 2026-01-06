using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Slugify;
using WebBanMayTinh.Authorization;
using WebBanMayTinh.Models;

namespace WebBanMayTinh.Areas.Admin.Controllers
{
    [Area("Admin")]
    [HasPermission(CustomClaimTypes.Permission, Permissions.BrandAccess)]
    public class BrandController : Controller
    {
        private readonly DataContext _context;

        public BrandController(DataContext context)
        {
            _context = context;
        }

        [HasPermission(CustomClaimTypes.Permission, Permissions.BrandRead)]
        public async Task<IActionResult> Index()
        {   
            return View(await _context.Brand.ToListAsync());
        }

        [HasPermission(CustomClaimTypes.Permission, Permissions.BrandRead)]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var brand = await _context.Brand
                .FirstOrDefaultAsync(m => m.Id == id);
            if (brand == null)
            {
                return NotFound();
            }

            return View(brand);
        }

        [HasPermission(CustomClaimTypes.Permission, Permissions.BrandCreate)]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [HasPermission(CustomClaimTypes.Permission, Permissions.BrandCreate)]
        public async Task<IActionResult> Create(Brand brand)
        {
            if (ModelState.IsValid)
            {
                var slug = new SlugHelper().GenerateSlug(brand.Name ?? "");

                var existedSlug = await _context.Brand.AnyAsync(m => m.Slug == slug);  

                if (existedSlug)
                {
                    ModelState.AddModelError("Name", "Hãng đã tồn tại");
                    return View(brand);
                }

                brand.Slug = slug;

                _context.Add(brand);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(brand);
        }

        [HasPermission(CustomClaimTypes.Permission, Permissions.BrandUpdate)]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var brand = await _context.Brand.FindAsync(id);
            if (brand == null)
            {
                return NotFound();
            }
            return View(brand);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [HasPermission(CustomClaimTypes.Permission, Permissions.BrandUpdate)]
        public async Task<IActionResult> Edit(int id, Brand brand)
        {
            if (id != brand.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(brand);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BrandExists(brand.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(brand);
        }

        [HasPermission(CustomClaimTypes.Permission, Permissions.BrandDelete)]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var brand = await _context.Brand
                .FirstOrDefaultAsync(m => m.Id == id);
            if (brand == null)
            {
                return NotFound();
            }

            return View(brand);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [HasPermission(CustomClaimTypes.Permission, Permissions.BrandDelete)]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // Lấy Brand cùng các sản phẩm liên quan
            var brand = await _context.Brand
                .Include(b => b.Products)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (brand == null) return NotFound();

            // Set BrandId của các product liên quan = null
            if (brand.Products != null && brand.Products.Any())
            {
                foreach (var product in brand.Products)
                {
                    product.BrandId = null;
                }
            }

            // Xóa Brand
            _context.Brand.Remove(brand);

            await _context.SaveChangesAsync();

            TempData["Success"] = "Xóa thương hiệu thành công, các sản phẩm vẫn giữ nguyên.";
            return RedirectToAction(nameof(Index));
        }


        private bool BrandExists(int id)
        {
            return _context.Brand.Any(e => e.Id == id);
        }
    }
}
