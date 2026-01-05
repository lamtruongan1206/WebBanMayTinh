using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebBanMayTinh.Models;

namespace WebBanMayTinh.Controllers
{
    [Authorize]
    public class AddressController : Controller
    {
        private readonly DataContext _context;
        private readonly UserManager<AppUser> userManager;

        public AddressController(DataContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            this.userManager = userManager;
        }

        private async Task<AppUser?> getUser ()
        {
            return await userManager.GetUserAsync(User);
        }

        // GET: Address
        public async Task<IActionResult> Index()
        {
            AppUser user = await getUser();
            if (user == null) return RedirectToAction("Login", "Account");
            var dataContext = _context.Addresses.Include(a => a.User).Where(a => a.UserId == user.Id && a.IsActive == true);
            return View(await dataContext.ToListAsync());
        }

        // GET: Address/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var address = await _context.Addresses
                .Include(a => a.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (address == null)
            {
                return NotFound();
            }

            return View(address);
        }

        // GET: Address/Create
        public IActionResult Create(string returnUrl = "Index")
        {
            ViewBag.ReturnUrl = returnUrl;
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id");
            return View();
        }

        // POST: Address/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Address address, string returnUrl)
        {
            AppUser? user = await userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (ModelState.IsValid)
            {
                address.UserId = user.Id;
                address.Id = Guid.NewGuid();
                _context.Add(address);
                
                if (address.IsDefault)
                {
                    var defautAddress = await _context.Addresses.FirstOrDefaultAsync(a => a.IsDefault == true && a.UserId == user.Id);
                    if (defautAddress != null)
                    {
                        defautAddress.IsDefault = false;
                        _context.Update(defautAddress);
                    }
                }

                await _context.SaveChangesAsync();
                return Redirect(returnUrl);
            }
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", address.UserId);
            return Redirect(returnUrl);
        }

        // GET: Address/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var address = await _context.Addresses.FindAsync(id);
            if (address == null)
            {
                return NotFound();
            }
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", address.UserId);
            return View(address);
        }

        // POST: Address/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, Address address)
        {
            var user = await getUser();

            if (id != address.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (address.IsDefault)
                    {
                        var defaultAddress = await _context.Addresses.FirstOrDefaultAsync(
                            a => a.IsDefault == true && 
                            a.Id != address.Id && 
                            a.UserId == user.Id);


                        if (defaultAddress != null)
                        {
                            defaultAddress.IsDefault = false;
                            _context.Update(defaultAddress);
                        }
                    }

                    _context.Update(address);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AddressExists(address.Id))
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
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", address.UserId);
            return View(address);
        }

        // GET: Address/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var address = await _context.Addresses
                .Include(a => a.User)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (address == null)
            {
                return NotFound();
            }

            return View(address);
        }

        // POST: Address/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var address = await _context.Addresses.FindAsync(id);
            if (address != null)
            {
                address.IsActive = false;
                await _context.SaveChangesAsync();

            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AddressExists(Guid id)
        {
            return _context.Addresses.Any(e => e.Id == id);
        }
    }
}
