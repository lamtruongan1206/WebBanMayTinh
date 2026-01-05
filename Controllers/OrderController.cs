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
using WebBanMayTinh.Models.Views;
using WebBanMayTinh.Services;

namespace WebBanMayTinh.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private readonly DataContext _context;
        private readonly UserManager<AppUser> _userManage;
        private readonly IUserService _userService;

        public OrderController(DataContext context, UserManager<AppUser> userManage, IUserService userService)
        {
            _context = context;
            _userManage = userManage;
            _userService = userService;
        }

        private async Task<AppUser?> GetCurrentUser()
        {
            return await _userManage.GetUserAsync(User);
        }

        public async Task<IActionResult> Index()
        {
            var user = await GetCurrentUser();
            var orderVMs = await _context.Orders
                .Where(o => o.UserId == user.Id)
                .Select(o => new OrderVM
                {
                    Id = o.Id,
                    OrderStatus = o.Status,
                    TotalAmount = o.TotalAmount,
                    Quantity = o.Quantity,
                    IsCancelRequested = o.IsCancelRequested,
                    Items = o.OrderItems.Select(oi => new OrderItemVM
                    {
                        ProductThumbnailUrl = oi.Product.ThumbnailUrl,
                        ProductName = oi.Product.Name,
                        Price = oi.Price,
                        Quantity = oi.Quantity,
                    }).ToList(),
                }).ToListAsync();

            return View(orderVMs);
        }

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
            var vm = new OrderDetailVM
            {
                Id = order.Id,
                Address = order.Address,
                User = order.User,
                AddressId = order.AddressId,
                UserId = order.User.Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Status = order.Status,
                Items = order.OrderItems
                    .Select(oi => new OrderItemVM
                        {
                            ProductName = oi.Product.Name!,
                            Price = oi.Price,
                            Quantity = oi.Quantity,
                        }).ToList()
            };

            return View(vm);
        }

        public IActionResult Create()
        {
            ViewData["AddressId"] = new SelectList(_context.Addresses, "Id", "AddressLine");
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,CreatedAt,UpdatedAt,Status,Subtotal,ShippingFee,TotalAmount,UserId,AddressId")] Order order)
        {
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

        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }
            ViewData["AddressId"] = new SelectList(_context.Addresses, "Id", "AddressLine", order.AddressId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", order.UserId);
            return View(order);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,CreatedAt,UpdatedAt,Status,Subtotal,ShippingFee,TotalAmount,UserId,AddressId")] Order order)
        {
            if (id != order.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(order);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OrderExists(order.Id))
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
            ViewData["AddressId"] = new SelectList(_context.Addresses, "Id", "AddressLine", order.AddressId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", order.UserId);
            return View(order);
        }

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

        [HttpGet]
        public async Task<IActionResult> CancelOrderRequest(Guid orderId)
        {
            var order = await _context.Orders.FindAsync(orderId);

            if (order == null)
                return NotFound();

            // Không cho yêu cầu hủy nếu đã giao hoặc đã hủy
            if (order.Status == OrderStatus.Completed || order.Status == OrderStatus.Cancelled && !order.IsCancelRequested)
                return BadRequest("Không thể hủy đơn hàng này");
            
            var vm = new CancelRequestVM
            {
                OrderId = order.Id
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelOrderRequest(CancelRequestVM vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            try
            {
                var order = await _context.Orders.FindAsync(vm.OrderId);

                if (order == null)
                    return NotFound();

                order.CancelReason = vm.CancelReason;
                order.IsCancelRequested = true;
                order.CancelRequestedAt = DateTime.Now;
                //order.Status = OrderStatus.CancelRequested;

                _context.Update(order);

                TempData["Success"] = "Đã gửi yêu cầu hủy đơn hàng";
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Không thể gửi yêu cầu hủy đơn hàng, vui lòng liên hệ chủ cửa hàng";
            }


            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmReceived(Guid id)
        {
            try
            {
                var order = await _context.Orders.FindAsync(id);

                if (order == null)
                    return NotFound();

                order.Status = OrderStatus.Completed;

                _context.Update(order);

                //TempData["Success"] = "Đã gửi yêu cầu hủy đơn hàng";
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Lỗi hệ thống, vui lòng thử lại";
            }
            return RedirectToAction("Index");
        }

    }
}
