using System;
using System.Linq;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebBanMayTinh.Models;
using WebBanMayTinh.Models.DTO;
using WebBanMayTinh.Models.Views;

namespace WebBanMayTinh.Controllers
{
    public class CartController : Controller
    {
        private readonly DataContext _context;
        private readonly UserManager<AppUser> _userManager;

        public CartController(DataContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // ================== UPDATE SESSION CART COUNT ==================
        private void UpdateCartSession()
        {
            var user = _userManager.GetUserAsync(User).Result;
            if (user == null) return;

            int count = _context.Carts
                .Where(c => c.UserId == user.Id)
                .Sum(c => c.Quantity ?? 0);

            HttpContext.Session.SetInt32("CartCount", count);
        }

        // ================== ADD PRODUCT ==================
        public IActionResult Add(Guid productId)
        {
            var user = _userManager.GetUserAsync(User).Result;
            if (user == null) return Redirect("/Account/Login");

            var product = _context.Products.Find(productId);
            if (product == null || product.Quantity <= 0)
            {
                TempData["Error"] = "Sản phẩm đã hết hàng!";
                return RedirectToAction("Index", "Product");
            }

            var cart = _context.Carts
                .FirstOrDefault(c => c.UserId == user.Id && c.ProductId == productId);

            if (cart != null)
            {
                if (cart.Quantity + 1 > product.Quantity)
                {
                    cart.Quantity = product.Quantity;
                    TempData["Error"] = $"Chỉ còn {product.Quantity} sản phẩm trong kho!";
                }
                else
                {
                    cart.Quantity += 1;
                    TempData["AddSuccess"] = "Đã tăng số lượng sản phẩm!";
                }
            }
            else
            {
                _context.Carts.Add(new Cart
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    ProductId = productId,
                    Quantity = 1,
                    IsSelected = true,
                    CreateAt = DateOnly.FromDateTime(DateTime.Now)
                });
                TempData["AddSuccess"] = "Đã thêm sản phẩm vào giỏ hàng!";
            }

            _context.SaveChanges();
            UpdateCartSession();
            return RedirectToAction("Index", "Product");
        }

        // ================== VIEW CART ==================
        public IActionResult Index()
        {
            var user = _userManager.GetUserAsync(User).Result;
            if (user == null) return Redirect("/Account/Login");

            var carts = _context.Carts
                .Include(c => c.Product)
                    .ThenInclude(p => p.Images)
                .Where(c => c.UserId == user.Id)
                .ToList();

            var data = carts.Select(c => new CartVM
            {
                CartId = c.Id,
                ProductId = c.ProductId ?? Guid.Empty,
                Name = c.Product.Name,
                Price = c.Product.Price ?? 0,
                Image = c.Product.Images.FirstOrDefault(i => i.IsMain)?.Url ?? "/images/no-image.png",
                Quantity = c.Quantity ?? 1,
                Checked = c.IsSelected ?? false
            }).ToList();

            return View(data);
        }

        // ================== REMOVE ==================
        public IActionResult Remove(Guid cartId)
        {
            var user = _userManager.GetUserAsync(User).Result;
            if (user == null) return Redirect("/Account/Login");

            var cart = _context.Carts.FirstOrDefault(c => c.Id == cartId && c.UserId == user.Id);
            if (cart != null)
            {
                _context.Carts.Remove(cart);
                _context.SaveChanges();
                UpdateCartSession();
            }

            return RedirectToAction("Index");
        }

        // ================== UPDATE QUANTITY (CORE LOGIC) ==================
        [HttpPost]
        public IActionResult UpdateQuantity([FromBody] CartUpdateQuantityVM vm)
        {
            var user = _userManager.GetUserAsync(User).Result;
            if (user == null) return Unauthorized();

            var cart = _context.Carts
                .Include(c => c.Product)
                .FirstOrDefault(c => c.Id == vm.CartId && c.UserId == user.Id);

            if (cart == null) return NotFound();

            int maxStock = cart.Product.Quantity ?? 0;

            if (vm.Quantity <= 0)
            {
                _context.Carts.Remove(cart);
                _context.SaveChanges();
                UpdateCartSession();
                return Json(new { removed = true });
            }

            if (vm.Quantity > maxStock)
            {
                cart.Quantity = maxStock;
                _context.SaveChanges();
                UpdateCartSession();

                return Json(new
                {
                    success = false,
                    message = $"Số lượng tối đa còn lại là {maxStock}",
                    quantity = maxStock
                });
            }

            cart.Quantity = vm.Quantity;
            _context.SaveChanges();
            UpdateCartSession();

            return Json(new { success = true, quantity = vm.Quantity });
        }

        // ================== TOGGLE SELECT ==================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ToggleSelect([FromBody] ToggleCartVM vm)
        {
            var user = _userManager.GetUserAsync(User).Result;
            if (user == null) return Unauthorized();

            var cart = _context.Carts.FirstOrDefault(c => c.Id == vm.CartId && c.UserId == user.Id);
            if (cart == null) return NotFound();

            cart.IsSelected = vm.Selected;
            _context.SaveChanges();

            return Ok();
        }
    }
}
