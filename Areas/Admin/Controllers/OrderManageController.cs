using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebBanMayTinh.Areas.Admin.Models.Views;
using WebBanMayTinh.Models;

namespace WebBanMayTinh.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class OrderManageController : Controller
    {
        private readonly DataContext _context;

        public OrderManageController(DataContext context)
        {
            _context = context;
        }

        private void InitViewBagStatus(OrderStatus currentStatus = OrderStatus.Pending)
        {
            var list = Enum.GetValues(typeof(OrderStatus))
            .Cast<OrderStatus>()
            .Where(s => (int)s > (int)currentStatus)   // CHỖ QUAN TRỌNG
            .Select(s => new
            {
                Id = (int)s,
                Name = s.ToString()
            })
            .ToList();
            ViewBag.OrderStatus = new SelectList(list, "Id", "Name");
        }

        // GET: Admin/OrderManage
        public async Task<IActionResult> Index()
        {
            var dataContext = _context.Orders.Include(o => o.Address).Include(o => o.User).OrderByDescending(o => o.CreatedAt);
            return View(await dataContext.ToListAsync());
        }

        // GET: Admin/OrderManage/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.Address)
                .Include(o => o.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // GET: Admin/OrderManage/Create
        public IActionResult Create()
        {
            InitViewBagStatus();
            ViewData["AddressId"] = new SelectList(_context.Addresses, "Id", "AddressLine");
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id");
            return View();
        }

        // POST: Admin/OrderManage/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Order order)
        {
            InitViewBagStatus();
            if (ModelState.IsValid)
            {
                order.Id = Guid.NewGuid();
                _context.Add(order);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["AddressId"] = new SelectList(_context.Addresses, "Id", "AddressLine", order.AddressId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", order.UserId);
            return View(order);
        }

        // GET: Admin/OrderManage/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {

            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders.FindAsync(id);

            InitViewBagStatus(order.Status);

            if (order == null)
            {
                return NotFound();
            }

            return View(new OrderEditVM
            {
                OrderId = order.Id,
                Status = order.Status
            });
        }

        // POST: Admin/OrderManage/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, OrderEditVM vm)
        {
            InitViewBagStatus();
            if (id != vm.OrderId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var order = _context.Orders.Find(id);
                    order.Status = vm.Status;
                    _context.Update(order);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OrderExists(vm.OrderId))
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
            return View(vm);
        }

        // GET: Admin/OrderManage/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.Address)
                .Include(o => o.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // POST: Admin/OrderManage/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order != null)
            {
                _context.Orders.Remove(order);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool OrderExists(Guid id)
        {
            return _context.Orders.Any(e => e.Id == id);
        }
    }
}
