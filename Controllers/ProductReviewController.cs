using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebBanMayTinh.Models;
using WebBanMayTinh.Models.Views;
using WebBanMayTinh.Services;

namespace WebBanMayTinh.Controllers
{
    public class ProductReviewController : Controller
    {
        private readonly DataContext _context;
        private readonly IUserService _userService;

        public ProductReviewController(
            DataContext dataContext, IUserService userService)
        {
            _context = dataContext;
            _userService = userService;
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateProductReviewVM model)
        {
            var user = await _userService.GetCurrentUser();

            if (user is null) return NotFound();

            var userId = user.Id;

            foreach (var item in model.Reviews)
            {
                var order = await _context.Orders.FindAsync(model.OrderId);

                if (order != null)
                {
                    if (order.IsReviewed) continue;
                    
                    order.IsReviewed = true;
                    _context.Update(order);
                }
                var review = new ProductReview
                {
                    OrderId = model.OrderId,
                    ProductId = item.ProductId,
                    UserId = userId,
                    Rating = item.Rating,
                    Comment = item.Comment,
                    CreatedAt = DateTime.Now,
                    IsApproved = false
                };
                _context.ProductReviews.Add(review);
            }

            await _context.SaveChangesAsync();

            return Redirect("/Order");
        }
    }
}
