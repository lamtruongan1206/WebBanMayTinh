using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebBanMayTinh.Areas.Admin.Models.Views;
using WebBanMayTinh.Authorization;
using WebBanMayTinh.Models;
using WebBanMayTinh.Utils;

namespace WebBanMayTinh.Areas.Admin.Controllers
{
    [Area("Admin")]
    [HasPermission(CustomClaimTypes.Permission, Permissions.OrderAccess)]
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
                Name = OrderStatusExtensions.ToVietNamText(s)
            })
            .ToList();
            ViewBag.OrderStatus = new SelectList(list, "Id", "Name");
        }

        [HasPermission(CustomClaimTypes.Permission, Permissions.OrderRead)]
        public async Task<IActionResult> Index(
            int? Status,
            DateTime? FromDate,
            DateTime? ToDate,
            string? UserName,
            decimal? TotalFrom,
            decimal? TotalTo,
            int page = 1,
            int pageSize = 4)
        {
            IQueryable<Order> query = _context.Orders
                .Include(o => o.Address)
                .Include(o => o.User);


            if (Status.HasValue)
            {
                query = query.Where(o => (int)o.Status == Status.Value);
            }

            if (FromDate.HasValue)
            {
                query = query.Where(o => o.CreatedAt >= FromDate.Value);
            }

            if (ToDate.HasValue)
            {
                query = query.Where(o => o.CreatedAt <= ToDate.Value.AddDays(1));
            }

            if (!string.IsNullOrWhiteSpace(UserName))
            {
                query = query.Where(o =>
                    o.User.UserName.Contains(UserName) ||
                    o.User.Email.Contains(UserName));
            }

            if (TotalFrom.HasValue)
            {
                query = query.Where(o => o.TotalAmount >= TotalFrom.Value);
            }

            if (TotalTo.HasValue)
            {
                query = query.Where(o => o.TotalAmount <= TotalTo.Value);
            }


            var pagedOrders = await query
                .OrderByDescending(o => o.CreatedAt)
                .ToPagedResultAsync(page, pageSize);

            ViewBag.StatusFrom = Status;
            ViewBag.FromDate = FromDate?.ToString("yyyy-MM-dd");
            ViewBag.ToDate = ToDate?.ToString("yyyy-MM-dd");
            ViewBag.UserName = UserName;
            ViewBag.TotalFrom = TotalFrom;
            ViewBag.TotalTo = TotalTo;

            return View(pagedOrders);
        }

        [HasPermission(CustomClaimTypes.Permission, Permissions.OrderRead)]
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.Address)
                .Include(o => o.User)
                .Include(o => o.OrderItems).ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        [HasPermission(CustomClaimTypes.Permission, Permissions.OrderCreate)]
        public IActionResult Create()
        {
            InitViewBagStatus();
            ViewData["AddressId"] = new SelectList(_context.Addresses, "Id", "AddressLine");
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [HasPermission(CustomClaimTypes.Permission, Permissions.OrderCreate)]
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

        [HasPermission(CustomClaimTypes.Permission, Permissions.OrderUpdate)]
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        [HasPermission(CustomClaimTypes.Permission, Permissions.OrderUpdate)]
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

                    if (order is null) return NotFound("Không tìm thấy đơn hàng này");

                    order.Status = vm.Status;
                    order.UpdatedAt = DateTime.Now;

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

        [HasPermission(CustomClaimTypes.Permission, Permissions.OrderDelete)]
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.Address)
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        [HttpPost, ActionName("Delete")]
        [HasPermission(CustomClaimTypes.Permission, Permissions.OrderDelete)]
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        [HasPermission(CustomClaimTypes.Permission, Permissions.OrderUpdate)]
        public async Task<IActionResult> ConfirmCancelRequest(Guid id)
        {
            try
            {
                var order = await _context.Orders.FindAsync(id);
                if (order is null)
                {
                    return NotFound("Không tìm thấy đơn hàng");
                }
                else if (order.CancelledAt != null || order.Status == OrderStatus.Cancelled)
                {
                    TempData["Error"] = "Đơn hàng đã được hủy trước đó rồi";
                    return RedirectToAction("Index");
                }

                order.UpdatedAt = DateTime.Now;
                order.CancelledAt = DateTime.Now;
                order.Status = OrderStatus.Cancelled;

                TempData["Success"] = "Hủy đơn hàng thành công";

                _context.Update(order);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Hủy đơn hàng không thành công";
            }
            return RedirectToAction("Index");
        }


        private bool OrderExists(Guid id)
        {
            return _context.Orders.Any(e => e.Id == id);
        }
    }
}
