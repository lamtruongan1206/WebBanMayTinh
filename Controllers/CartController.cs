using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebBanMayTinh.Models;
using WebBanMayTinh.Models.DTO;

public class CartController : Controller
{
    DataContext conn;
    UserManager<AppUser> userManager;

    public CartController(
        DataContext context,
        UserManager<AppUser> userManager)
    {
        this.conn = context;
        this.userManager = userManager;
    }

    // Hàm cập nhật lại session CartCount
    private void UpdateCartSession(string userId)
    {
        int count = conn.Carts
            .Where(c => c.UserId == userId)
            .Sum(c => c.Quantity ?? 0);   

        HttpContext.Session.SetInt32("CartCount", count);
    }

    // 1) Thêm sản phẩm vào giỏ
    public async Task<IActionResult> Add(Guid productId)
    {
        var user = await userManager.GetUserAsync(User);

        if (user == null)
            return RedirectToAction("Login", "Login"); // nếu ấn vào nút thêm vào giỏ hàng ở trang chưa đăng nhập thì phải vào đăng nhập 

        var item = conn.Carts.FirstOrDefault(
            c => c.UserId == user.Id && c.ProductId == productId);

        if (item != null)
        {
            item.Quantity += 1;
        }

        else
        {
            conn.Carts.Add(new Cart
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                ProductId = productId,
                Quantity = 1,
                CreateAt = DateOnly.FromDateTime(DateTime.Now)
            });
        }

        conn.SaveChanges();
        // Cập nhật session
        UpdateCartSession(user.Id);

        TempData["AddSuccess"] = "Đã thêm vào giỏ hàng thành công!";
        return RedirectToAction("Index", "Product");
    }

    // 2) Hiển thị giỏ hàng
    public async Task<IActionResult> Index()
    {
        var user = await userManager.GetUserAsync(User);


        if (user == null)
        {
            return Redirect("/account/login");
        }

        var userId = user.Id;

        if (string.IsNullOrEmpty(userId))
            return RedirectToAction("Login", "Login");

        var data = conn.Carts
            .Where(c => c.UserId == userId)
            .Select(c => new CartVM
            {
                CartId = c.Id,
                ProductId = c.ProductId ?? Guid.Empty,
                Name = c.Product.Name,
                Price = c.Product.Price ?? 0,
                Image = c.Product.Images
                    .Where(i => i.IsMain)
                    .Select(i => i.Url)
                    .FirstOrDefault()
                    ?? "/images/no-image.png",
                Quantity = c.Quantity ?? 1
            })
            .ToList();

        return View(data);
    }

    // 3) Xoá sản phẩm
    public async Task<IActionResult> Remove(Guid cartId)
    {
        var user = await userManager.GetUserAsync(User);
        var userId = user.Id;
        if (userId == null)
            return RedirectToAction("Login", "Login");

        var item = conn.Carts.FirstOrDefault(c => c.Id == cartId && c.UserId == userId);

        if (item != null)
        {
            conn.Carts.Remove(item);
            conn.SaveChanges();

            // Cập nhật lại session
            UpdateCartSession(userId);
        }

        return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> UpdateQuantity(Guid cartId, int quantity)
    {
        var user = await userManager.GetUserAsync(User);
        var userId = user.Id;
        if (userId == null)
            return RedirectToAction("Login", "Login");

        var item = conn.Carts.FirstOrDefault(c => c.Id == cartId);
        if (item == null) return RedirectToAction("Index");

        if (quantity <= 0)
            conn.Carts.Remove(item);
        else
            item.Quantity = quantity;

        conn.SaveChanges();
        UpdateCartSession(userId);

        return RedirectToAction("Index");
    }

}
