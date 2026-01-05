using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using WebBanMayTinh.Models;
using WebBanMayTinh.Models.Views;

namespace WebBanMayTinh.Controllers
{
    [Authorize]
    public class CheckoutController : Controller
    {
        private readonly DataContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly IHttpClientFactory _httpClientFactory;

        // ================== MoMo CONFIG (TEST) ==================
        private const string MOMO_ENDPOINT = "https://test-payment.momo.vn/v2/gateway/api/create";
        private const string MOMO_PARTNER_CODE = "MOMO";
        private const string MOMO_ACCESS_KEY = "F8BBA842ECF85";
        private const string MOMO_SECRET_KEY = "K951B6PE1waDMi640xX08PD3vg6EkVlz";
        private const string MOMO_RETURN_URL = "https://localhost:7212/Checkout/MomoReturn";
        private const string MOMO_NOTIFY_URL = "https://localhost:7212/Checkout/MomoNotify";

        public CheckoutController(
            DataContext context,
            UserManager<AppUser> userManager,
            IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _userManager = userManager;
            _httpClientFactory = httpClientFactory;
        }

        // =========================================================
        // HIỂN THỊ CHECKOUT
        // =========================================================
        public IActionResult Index()
        {
            var user = _userManager.GetUserAsync(User).Result;
            if (user == null) return Redirect("/Account/Login");

            decimal shippingFee = 100000;
            decimal subtotal = 0;

            // Địa chỉ
            var addressId = HttpContext.Session.GetString("SelectedAddressId");
            var address = addressId != null
                ? _context.Addresses.FirstOrDefault(a => a.Id == Guid.Parse(addressId))
                : _context.Addresses.FirstOrDefault(a => a.UserId == user.Id && a.IsDefault == true);

            // Giỏ hàng được chọn
            var carts = _context.Carts
                .Include(c => c.Product)
                .Where(c => c.UserId == user.Id && c.IsSelected == true)
                .ToList();

            List<ProductCheckoutVM> products = new();

            foreach (var c in carts)
            {
                subtotal += (c.Product.Price ?? 0) * (c.Quantity ?? 0);

                products.Add(new ProductCheckoutVM
                {
                    Name = c.Product.Name,
                    Price = c.Product.Price ?? 0,
                    OrderQuantity = c.Quantity ?? 0
                });
            }

            return View(new CheckoutVM
            {
                Products = products,
                Address = address,
                Subtotal = subtotal,
                ShippingFee = shippingFee,
                TotalAmount = subtotal + shippingFee
            });

        }

        // =========================================================
        // ĐẶT HÀNG
        // =========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult PlaceOrder(string paymentMethod)
        {
            var user = _userManager.GetUserAsync(User).Result;
            if (user == null) return Redirect("/Account/Login");

            var carts = _context.Carts
                .Include(c => c.Product)
                .Where(c => c.UserId == user.Id && c.IsSelected == true)
                .ToList();

            if (!carts.Any())
            {
                TempData["Error"] = "Giỏ hàng trống!";
                return RedirectToAction("Index");
            }

            var addressId = HttpContext.Session.GetString("SelectedAddressId");
            var address = addressId != null
                ? _context.Addresses.Find(Guid.Parse(addressId))
                : _context.Addresses.FirstOrDefault(a => a.UserId == user.Id && a.IsDefault);

            if (address == null)
                return Redirect("/Address/Create?returnUrl=/Checkout/Index");

            // 🔒 CHECK STOCK
            foreach (var c in carts)
            {
                if (c.Product.Quantity < c.Quantity)
                {
                    TempData["Error"] = $"Sản phẩm {c.Product.Name} không đủ hàng!";
                    return RedirectToAction("Index");
                }
            }

            // 🧾 TẠO ORDER CHUNG
            Order order = new()
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                AddressId = address.Id,
                Status = paymentMethod == "COD"
                            ? OrderStatus.Pending
                            : OrderStatus.Pending, // chờ thanh toán
                CreatedAt = DateTime.Now,
                OrderItems = new()
            };

            decimal subtotal = 0;
            int totalQty = 0;

            foreach (var c in carts)
            {
                subtotal += (c.Product.Price ?? 0) * (c.Quantity ?? 0);
                totalQty += c.Quantity ?? 0;

                order.OrderItems.Add(new OrderItems
                {
                    Id = Guid.NewGuid(),
                 //   OrderId = order.Id, // 🔥 BẮT BUỘC
                    ProductId = c.Product.Id,
                    Quantity = c.Quantity ?? 0,
                    Price = c.Product.Price ?? 0
                });

            }

            order.Subtotal = subtotal;
            order.ShippingFee = 100000;
            order.TotalAmount = subtotal + order.ShippingFee;
            order.Quantity = totalQty;

            // ================= COD =================
            if (paymentMethod == "COD")
            {
                foreach (var c in carts)
                {
                    c.Product.Quantity -= c.Quantity ?? 0;
                }

                _context.Orders.Add(order);
                _context.Carts.RemoveRange(carts);
                _context.SaveChanges();

                TempData["Success"] = "Đặt hàng COD thành công!";
                return RedirectToAction("Index", "Cart");
            }

            // ================= MOMO =================
            _context.Orders.Add(order);
            _context.SaveChanges();

            return CreateMoMoPayment(order);
        }


        // =========================================================
        // TẠO THANH TOÁN MOMO
        // =========================================================
        private IActionResult CreateMoMoPayment(Order order)
        {
            string requestId = Guid.NewGuid().ToString();
            string orderId = order.Id.ToString();
            long amount = (long)order.TotalAmount;

            string rawHash =
                $"accessKey={MOMO_ACCESS_KEY}" +
                $"&amount={amount}" +
                $"&extraData=" +
                $"&ipnUrl={MOMO_NOTIFY_URL}" +
                $"&orderId={orderId}" +
                $"&orderInfo=Thanh toán đơn hàng" +
                $"&partnerCode={MOMO_PARTNER_CODE}" +
                $"&redirectUrl={MOMO_RETURN_URL}" +
                $"&requestId={requestId}" +
                $"&requestType=captureWallet";

            string signature = HmacSHA256(rawHash, MOMO_SECRET_KEY);

            var body = new
            {
                partnerCode = MOMO_PARTNER_CODE,
                accessKey = MOMO_ACCESS_KEY,
                requestId,
                amount,
                orderId,
                orderInfo = "Thanh toán đơn hàng",
                redirectUrl = MOMO_RETURN_URL,
                ipnUrl = MOMO_NOTIFY_URL,
                requestType = "captureWallet",
                extraData = "",
                signature
            };

            var client = _httpClientFactory.CreateClient();
            var res = client.PostAsync(
                MOMO_ENDPOINT,
                new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json")
            ).Result;

            var json = JsonDocument.Parse(res.Content.ReadAsStringAsync().Result);
            var root = json.RootElement;

            if (root.GetProperty("resultCode").GetInt32() != 0)
            {
                TempData["Error"] = "Không tạo được thanh toán MoMo";
                return RedirectToAction("Index");
            }

            return Redirect(root.GetProperty("payUrl").GetString());
        }


        // =========================================================
        // MOMO CALLBACK
        // =========================================================
        public IActionResult MomoReturn(Guid orderId, int resultCode)
        {
            var order = _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(i => i.Product)
                .FirstOrDefault(o => o.Id == orderId);

            if (order == null)
                return NotFound();

            // 🔒 CHỐNG CALLBACK TRÙNG
            if (order.Status == OrderStatus.Confirmed)
            {
                return RedirectToAction("Index", "Cart");
            }

            // 🔒 CHỈ XỬ LÝ ĐƠN ĐANG CHỜ THANH TOÁN
            if (order.Status != OrderStatus.Pending)
            {
                TempData["Error"] = "Đơn hàng không hợp lệ!";
                return RedirectToAction("Index", "Cart");
            }

            if (resultCode == 0)
            {
                foreach (var item in order.OrderItems)
                {
                    item.Product.Quantity -= item.Quantity;
                }

                order.Status = OrderStatus.Confirmed;

                var carts = _context.Carts
                    .Where(c => c.UserId == order.UserId && c.IsSelected == true);

                _context.Carts.RemoveRange(carts);
                _context.SaveChanges();

                TempData["Success"] = "Thanh toán MoMo thành công!";
            }
            else
            {
                order.Status = OrderStatus.Cancelled;
                _context.SaveChanges();

                TempData["Error"] = "Thanh toán MoMo thất bại!";
            }

            return RedirectToAction("Index", "Cart");
        }



        // =========================================================
        // CHANGE ADDRESS
        // =========================================================
        public IActionResult ChangeAddress()
        {
            var user = _userManager.GetUserAsync(User).Result;
            if (user == null) return Redirect("/Account/Login");

            var addresses = _context.Addresses
                .Where(a => a.UserId == user.Id && a.IsActive == true)
                .ToList();

            return View(addresses);
        }

        public IActionResult SelectAddress(Guid id)
        {
            HttpContext.Session.SetString("SelectedAddressId", id.ToString());
            return RedirectToAction("Index");
        }

        // =========================================================
        // HMAC SHA256
        // =========================================================
        private static string HmacSHA256(string data, string key)
        {
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key));
            return BitConverter.ToString(hmac.ComputeHash(Encoding.UTF8.GetBytes(data)))
                .Replace("-", "").ToLower();
        }
    }
}
