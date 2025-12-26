using Microsoft.AspNetCore.Mvc;
using WebBanMayTinh.Models;
using WebBanMayTinh.Models.DTO;
using WebBanMayTinh.Models.Views;

namespace WebBanMayTinh.Controllers
{
    public class CheckoutController : Controller
    {
        private DataContext dataContext;

        public CheckoutController(DataContext dataContext)
        {
            this.dataContext = dataContext;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(List<CartVM> vms)
        {
            List<Product> products = new List<Product>();
            decimal totalAmount = 0;

            foreach (CartVM cart in vms)
            {
                Product prod = dataContext.Products.FirstOrDefault(p => p.Id == cart.ProductId);

                if (prod != null && cart.Checked)
                {
                    totalAmount = totalAmount + prod.Price.Value;
                    products.Add(prod);
                } 
            }

            CheckoutVM checkoutVM = new CheckoutVM();
            
            checkoutVM.Products = products;
            checkoutVM.TotalAmount = totalAmount;
            checkoutVM.ShippingFee = 0;

            return View(checkoutVM);
        }
    }
}
