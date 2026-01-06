using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NuGet.Common;
using WebBanMayTinh.Authorization;
using WebBanMayTinh.Models;
using WebBanMayTinh.Utils;

namespace WebBanMayTinh.Areas.Admin.Controllers
{
    [Area("Admin")]
    [HasPermission(CustomClaimTypes.Permission, Permissions.SliderAccess)]
    public class SlidersController : Controller
    {
        private readonly DataContext _context;

        public SlidersController(DataContext context)
        {
            _context = context;
        }

        [HasPermission(CustomClaimTypes.Permission, Permissions.SliderRead)]
        public async Task<IActionResult> Index()
        {
            return View(await _context.Sliders.ToListAsync());
        }

        [HasPermission(CustomClaimTypes.Permission, Permissions.SliderRead)]
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var slider = await _context.Sliders
                .FirstOrDefaultAsync(m => m.Id == id);
            if (slider == null)
            {
                return NotFound();
            }

            return View(slider);
        }

        [HasPermission(CustomClaimTypes.Permission, Permissions.SliderCreate)]
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [HasPermission(CustomClaimTypes.Permission, Permissions.SliderCreate)]
        public async Task<IActionResult> Create(Slider slider, IFormFile fileImage)
        {
            if (ModelState.IsValid)
            {
                var path = await FileUtils.Upload(fileImage);
                slider.Id = Guid.NewGuid();
                slider.ImageUrl = path;
                _context.Add(slider);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(slider);
        }

        [HasPermission(CustomClaimTypes.Permission, Permissions.SliderUpdate)]
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var slider = await _context.Sliders.FindAsync(id);
            if (slider == null)
            {
                return NotFound();
            }
            return View(slider);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [HasPermission(CustomClaimTypes.Permission, Permissions.SliderUpdate)]
        public async Task<IActionResult> Edit(Guid id, Slider slider, IFormFile? fileImage)
        {
            if (id != slider.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (fileImage is not null)
                    {
                        var path = await FileUtils.Upload(fileImage);
                        slider.ImageUrl = path;
                    }
                    _context.Update(slider);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SliderExists(slider.Id))
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
            return View(slider);
        }

        [HasPermission(CustomClaimTypes.Permission, Permissions.SliderDelete)]
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var slider = await _context.Sliders
                .FirstOrDefaultAsync(m => m.Id == id);
            if (slider == null)
            {
                return NotFound();
            }

            return View(slider);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [HasPermission(CustomClaimTypes.Permission, Permissions.SliderDelete)]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var slider = await _context.Sliders.FindAsync(id);
            if (slider != null)
            {
                _context.Sliders.Remove(slider);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        private bool SliderExists(Guid id)
        {
            return _context.Sliders.Any(e => e.Id == id);
        }
    }
}
