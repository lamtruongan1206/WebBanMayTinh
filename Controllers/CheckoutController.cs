using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebBanMayTinh.Models;
using WebBanMayTinh.Models.DTO;
using WebBanMayTinh.Models.Views;

namespace WebBanMayTinh.Controllers
{
    [Authorize]
    public class CheckoutController : Controller
    {
        private DataContext dataContext;
        private UserManager<AppUser> UserManager;

        public CheckoutController(DataContext dataContext,
            UserManager<AppUser> userManager)
        {
            this.dataContext = dataContext;
            this.UserManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            List<ProductCheckoutVM> products = new List<ProductCheckoutVM>();
            decimal totalAmount = 0;
            var user = await UserManager.GetUserAsync(User);
            decimal shippingFee = 100000; // demo 10k

            if (user == null)
            {
                return Redirect("/account/login");
            }
            var selectedAddress = HttpContext.Session.GetString("SelectedAddressId");

            var defaultAddress = selectedAddress != null ? dataContext.Addresses.FirstOrDefault(a => a.Id == Guid.Parse(selectedAddress)) : dataContext.Addresses.FirstOrDefault(a => (a.IsDefault == true && a.UserId == user.Id));

            IEnumerable<Cart> carts = await dataContext.Carts.Where(c => c.IsSelected == true && c.UserId == user.Id).ToListAsync();

            foreach (var cart in carts)
            {
                Product prod = await dataContext.Products.FirstOrDefaultAsync(p => p.Id == cart.ProductId);
                if (prod == null) 
                {
                    return NotFound();
                }
                ProductCheckoutVM prodCheckoutVM = new ProductCheckoutVM();

                prodCheckoutVM.Name = prod.Name;
                prodCheckoutVM.OrderQuantity = cart.Quantity ?? 0;
                prodCheckoutVM.Price = prod.Price ?? 0;

                totalAmount += (prodCheckoutVM.OrderQuantity * prodCheckoutVM.Price);

                products.Add(prodCheckoutVM);
            }

            CheckoutVM checkoutVM = new CheckoutVM();
            
            checkoutVM.Products = products;
            checkoutVM.Address = defaultAddress;
            checkoutVM.TotalAmount = totalAmount;
            checkoutVM.ShippingFee = shippingFee;

            return View(checkoutVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add()
        {
            Order order = new Order();
            AppUser? user = await UserManager.GetUserAsync(User);
            decimal subTotal = 0;

            if (user == null)
            {
                return Redirect("/Account/Login");
            }

            IEnumerable<Cart> carts = await dataContext.Carts.Where(c => c.IsSelected == true && c.UserId == user.Id).ToListAsync();
            decimal shippingFee = 100000; // demo 10k

            if (user == null)
            {
                return NotFound();
            }

            var selectedAddress = HttpContext.Session.GetString("SelectedAddressId");

            var defaultAddress = selectedAddress != null ? dataContext.Addresses.FirstOrDefault(a => a.Id == Guid.Parse(selectedAddress)) : dataContext.Addresses.FirstOrDefault(a => (a.IsDefault == true && a.UserId == user.Id));

            if (defaultAddress == null)
            {
                return Redirect("/Address/Create?returnUrl=/Checkout/Index");
            }

            List<OrderItems> orderItems = new List<OrderItems>();

            foreach (var cart in carts)
            {
                Product prod = await dataContext.Products.FirstOrDefaultAsync(p => p.Id == cart.ProductId);
                if (prod == null)
                {
                    return NotFound();
                }

                subTotal = subTotal + prod.Price ?? 0;
                var discount = 1; //prod.Discount;

                var orderItem = new OrderItems
                {
                    Id = Guid.NewGuid(),
                    OrderId = order.Id,
                    Price = prod.Price * discount * cart.Quantity ?? 0, // Sẽ nhân thêm giảm giá nữa
                    ProductId = prod.Id,
                    Quantity = cart.Quantity ?? 0,
                };

                orderItems.Add(orderItem);
                dataContext.Carts.Remove(cart);
            }

            order.Subtotal = subTotal;
            order.ShippingFee = shippingFee;
            order.Address = defaultAddress;
            order.TotalAmount = subTotal + shippingFee;
            order.User = user;
            order.OrderItems = orderItems;

            try
            {
                dataContext.Orders.Add(order);
                dataContext.SaveChanges();
                TempData["Success"] = "Đơn hàng của bạn đã được tạo, vui lòng chờ duyệt.";
            }
            catch (Exception ex) 
            {
                TempData["Error"] = "Không thể tạo đơn hàng.";
            }

            return RedirectToAction("Index", "Cart");
        }
   
        public async Task<IActionResult> ChangeAddress()
        {
            AppUser user = await UserManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");
            var address = dataContext.Addresses.Include(a => a.User).Where(a => a.UserId == user.Id && a.IsActive == true);
            return View(await address.ToListAsync());
        }

        public async Task<IActionResult> SelectAddress(Guid id)
        {
            HttpContext.Session.SetString("SelectedAddressId", id.ToString());
            return RedirectToAction("Index");
        }

    }
}
