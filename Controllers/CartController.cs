using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebBanMayTinh.Models;
using WebBanMayTinh.Models.DTO;
using WebBanMayTinh.Models.Views;

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
                Quantity = c.Quantity ?? 1,
                Checked = c.IsSelected ?? false,
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
    public async Task<IActionResult> UpdateQuantity([FromBody] CartUpdateQuantityVM vm)
    {
        var user = await userManager.GetUserAsync(User);
        var userId = user.Id;
        if (userId == null)
            return RedirectToAction("Login", "Login");

        var item = conn.Carts.FirstOrDefault(c => c.Id == vm.CartId);
        if (item == null) return RedirectToAction("Index");

        if (vm.Quantity <= 0)
            conn.Carts.Remove(item);
        else
            item.Quantity = vm.Quantity;

        conn.SaveChanges();
        UpdateCartSession(userId);

        return RedirectToAction("Index");
    }

    public async Task<IActionResult> Select(Guid cartId)
    {
        var cart = await conn.Carts.FirstOrDefaultAsync(c => c.Id == cartId);

        if (cart == null)
        {
            return NotFound();
        } 

        cart.IsSelected = true;

        conn.SaveChanges();

        return RedirectToAction("Index");
    }

    private async Task<IEnumerable<Cart>> GetCarts ()
    {
        var user = await userManager.GetUserAsync(User);

        if (user == null)
            return Enumerable.Empty<Cart>();

        return conn.Carts.Where(c => c.UserId == user.Id);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleSelect([FromBody] ToggleCartVM model)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var cart = await conn.Carts
            .FirstOrDefaultAsync(c => c.Id == model.CartId && c.UserId == userId);

        if (cart == null) return NotFound();

        cart.IsSelected = model.Selected;
        await conn.SaveChangesAsync();

        return Ok();
    }

    

}
