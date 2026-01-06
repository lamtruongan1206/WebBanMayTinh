using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebBanMayTinh.Areas.Admin.Models.Views;
using WebBanMayTinh.Models;
using WebBanMayTinh.Models.DTO;

namespace WebBanMayTinh.Controllers
{
    public class ProductController : Controller
    {
        private readonly DataContext _context;
        private readonly UserManager<AppUser> _userManager;

        public ProductController(DataContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }


        // ================== INDEX ==================
        public IActionResult Index(
    string? searchName,
    string? searchBrand,
    decimal? priceFrom,
    decimal? priceTo,
    Guid? categoryId,
    int? brandId,
    int page = 1)
        {
            int pageSize = 6;

            // 1️⃣ Query gốc (chỉ lấy ảnh chính, AsNoTracking)
            var query = _context.Products
                .AsNoTracking()
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Include(p => p.Images.Where(i => i.IsMain))
                .Where(p => !p.IsDeleted)
                .AsQueryable();

            // 2️⃣ Filter
            if (!string.IsNullOrEmpty(searchName))
                query = query.Where(p => p.Name.Contains(searchName));

            if (!string.IsNullOrEmpty(searchBrand))
            {
                query = query.Where(c => c.Brand != null &&
                                         c.Brand.Name.Contains(searchBrand));
            }

            if (priceFrom.HasValue)
                query = query.Where(p => p.Price >= priceFrom.Value);

            if (priceTo.HasValue)
                query = query.Where(p => p.Price <= priceTo.Value);

            if (categoryId.HasValue)
                query = query.Where(p => p.CategoryId == categoryId);

            if (brandId.HasValue)
                query = query.Where(p => p.BrandId == brandId);

            // 3️⃣ Phân trang
            int totalItems = query.Count();
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var products = query
                .OrderBy(p => p.Name) // bắt buộc order để Skip/Take chính xác
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // 4️⃣ Sidebar data
            ViewBag.Categories = _context.Categories.AsNoTracking().ToList();
            ViewBag.Brands = _context.Brand.AsNoTracking().ToList();

            // 5️⃣ Lưu giá trị filter đang chọn
            ViewBag.SelectedCategory = categoryId;
            ViewBag.SelectedBrand = brandId;

            // 6️⃣ ViewBag cho form + paging
            ViewBag.SearchName = searchName;
            ViewBag.SearchBrand = searchBrand;
            ViewBag.PriceFrom = priceFrom;
            ViewBag.PriceTo = priceTo;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

            return View(products);
        }

        // ================== ADD ==================
        [HttpGet]
        public IActionResult Add()
        {
            ViewBag.Categories = new SelectList(_context.Categories, "Id", "Name");
            ViewBag.Brands = new SelectList(_context.Brand, "Id", "Name");
            return View(new ProductCreateVM());
        }

        [HttpPost]
        public IActionResult AddAction(ProductCreateVM dto)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.CategoriesId = new SelectList(_context.Categories, "Id", "Name", dto.CategoryId);
                ViewBag.BrandId = new SelectList(_context.Brand, "Id", "Name", dto.BrandId);
                return View(dto);
            }

            if (dto.CategoryId == null || !_context.Categories.Any(c => c.Id == dto.CategoryId))
            {
                ModelState.AddModelError("CategoriesId", "Danh mục không tồn tại");
                ViewBag.CategoriesId = new SelectList(_context.Categories, "Id", "Name", dto.CategoryId);
                return View(dto);
            }
            if (dto.BrandId == null || !_context.Brand.Any(b => b.Id == dto.BrandId))
            {
                ModelState.AddModelError("BrandId", "Thương hiệu không tồn tại");
                ViewBag.BrandId = new SelectList(_context.Brand, "Id", "Name", dto.BrandId);
                return View(dto);
            }

            // Tạo entity Computer
            var computer = new Product
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                Price = dto.Price,
                Quantity = dto.Quantity,
                Description = dto.Description,
                CategoryId = dto.CategoryId,
                BrandId = dto.BrandId,
                CreateAt = DateOnly.FromDateTime(DateTime.Now),
                UpdateAt = DateOnly.FromDateTime(DateTime.Now)
            };

            _context.Products.Add(computer);

            // Upload ảnh
            string wwwRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            string folder = Path.Combine(wwwRoot, "assets", "img", "Computer");
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

            // Ảnh chính
            if (dto.MainImage != null)
            {
                string mainFileName = Guid.NewGuid() + Path.GetExtension(dto.MainImage.FileName);
                string mainPath = Path.Combine(folder, mainFileName);
                using (var fs = new FileStream(mainPath, FileMode.Create))
                {
                    dto.MainImage.CopyTo(fs);
                }
                _context.Images.Add(new Image
                {
                    Id = Guid.NewGuid(),
                    ProductId = computer.Id,
                    Url = $"/assets/img/Computer/{mainFileName}",
                    IsMain = true
                });
            }

            // Ảnh phụ
            if (dto.AdditionalImages != null && dto.AdditionalImages.Any())
            {
                foreach (var file in dto.AdditionalImages)
                {
                    string fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
                    string filePath = Path.Combine(folder, fileName);
                    using (var fs = new FileStream(filePath, FileMode.Create))
                    {
                        file.CopyTo(fs);
                    }
                    _context.Images.Add(new Image
                    {
                        Id = Guid.NewGuid(),
                        ProductId = computer.Id,
                        Url = $"/assets/img/Computer/{fileName}",
                        IsMain = false
                    });
                }
            }

            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        // ================== UPDATE ==================
        [HttpGet]
        public IActionResult Update(Guid id)
        {
            var computer = _context.Products
                .Include(c => c.Images)
                .FirstOrDefault(c => c.Id == id);

            if (computer == null) return NotFound();

            ViewBag.CategoriesId = new SelectList(_context.Categories, "Id", "Name", computer.CategoryId);
            ViewBag.BrandId = new SelectList(_context.Brand, "Id", "Name", computer.BrandId);
            ViewBag.ComputerId = computer.Id;
            ViewBag.MainImage = computer.Images?.FirstOrDefault(i => i.IsMain)?.Url ?? "/images/default.png";
            var dto = new ComputerDto
            {
                Name = computer.Name,
                Manufacturer = computer.Manufacturer,
                Price = computer.Price,
                Quantity = computer.Quantity,
                Description = computer.Description,
                CategoriesId = computer.CategoryId,
                BrandId = computer.BrandId
            };
            return View(dto);
        }

        [HttpPost]
        public IActionResult UpdateAction(Guid id, ComputerDto dto)
        {
            var computer = _context.Products
                .Include(c => c.Images)
                .FirstOrDefault(c => c.Id == id);

            if (computer == null) return NotFound();

            if (!ModelState.IsValid)
            {
                ViewBag.BrandId = new SelectList(_context.Brand, "Id", "Name", dto.BrandId);
                ViewBag.CategoriesId = new SelectList(_context.Categories, "Id", "Name", dto.CategoriesId);
                ViewBag.MainImage = computer.Images?.FirstOrDefault(i => i.IsMain)?.Url ?? "/images/default.png";
                return View("Update" , dto);
            }

            computer.Name = dto.Name;
            computer.Manufacturer = dto.Manufacturer;
            computer.Price = dto.Price;
            computer.Quantity = dto.Quantity;
            computer.Description = dto.Description;
            computer.CategoryId = dto.CategoriesId;
            computer.BrandId = dto.BrandId;
            computer.UpdateAt = DateOnly.FromDateTime(DateTime.Now);

            string wwwRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            string folder = Path.Combine(wwwRoot, "assets", "img", "Computer");
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

            // Ảnh chính mới
            if (dto.MainImage != null)
            {
                var oldMain = computer.Images.FirstOrDefault(i => i.IsMain);
                if (oldMain != null)
                {
                    string oldPath = Path.Combine(wwwRoot, oldMain.Url.TrimStart('/').Replace("/", "\\"));
                    if (System.IO.File.Exists(oldPath)) System.IO.File.Delete(oldPath);
                    _context.Images.Remove(oldMain);
                }

                string fileName = Guid.NewGuid() + Path.GetExtension(dto.MainImage.FileName);
                string filePath = Path.Combine(folder, fileName);
                using (var fs = new FileStream(filePath, FileMode.Create))
                {
                    dto.MainImage.CopyTo(fs);
                }
                _context.Images.Add(new Image
                {
                    Id = Guid.NewGuid(),
                    ProductId = computer.Id,
                    Url = $"/assets/img/Computer/{fileName}",
                    IsMain = true
                });
            }

            // Ảnh phụ mới
            if (dto.AdditionalImages != null && dto.AdditionalImages.Any())
            {
                foreach (var file in dto.AdditionalImages)
                {
                    string fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
                    string filePath = Path.Combine(folder, fileName);
                    using (var fs = new FileStream(filePath, FileMode.Create))
                    {
                        file.CopyTo(fs);
                    }
                    _context.Images.Add(new Image
                    {
                        Id = Guid.NewGuid(),
                        ProductId = computer.Id,
                        Url = $"/assets/img/Computer/{fileName}",
                        IsMain = false
                    });
                }
            }

            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        // ================== DELETE ==================
        [HttpGet]
        public IActionResult Delete(Guid id)
        {
            var computer = _context.Products.FirstOrDefault(c => c.Id == id);
            if (computer == null) return NotFound();
            return View(computer);
        }

        [HttpPost]
        public IActionResult DeleteAction(Guid id)
        {
            var computer = _context.Products
                .Include(c => c.Images)
                .FirstOrDefault(c => c.Id == id);

            if (computer == null) return NotFound();

            string wwwRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");

            foreach (var img in computer.Images)
            {
                string fullPath = Path.Combine(wwwRoot, img.Url.TrimStart('/').Replace("/", "\\"));
                if (System.IO.File.Exists(fullPath)) System.IO.File.Delete(fullPath);
            }

            _context.Images.RemoveRange(computer.Images);
            _context.Products.Remove(computer);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }


        // ==================== DETAIL ===============
        [HttpGet]
        public IActionResult Detail(Guid id)
        {
            var computer = _context.Products
                .Include(c => c.Images)
                .Include(c => c.Category)
                .Include(c => c.Brand)
                .Include(c => c.Specifications)
                .FirstOrDefault(c => c.Id == id);

            if (computer == null) return NotFound();

            // Lấy sản phẩm liên quan: chỉ ảnh chính
            var relatedProducts = _context.Products
                .Where(p => p.Id != id &&
                            (p.CategoryId == computer.CategoryId || p.BrandId == computer.BrandId))
                .Select(p => new
                {
                    Product = p,
                    MainImageUrl = p.Images.FirstOrDefault(i => i.IsMain) != null
                                   ? p.Images.FirstOrDefault(i => i.IsMain).Url
                                   : "/images/default.png"
                })
                .Take(4)
                .ToList()
                .Select(x => {
                    x.Product.Images = new List<Image> { new Image { Url = x.MainImageUrl, IsMain = true } };
                    return x.Product;
                })
                .ToList();

            ViewBag.RelatedProducts = relatedProducts;

            return View(computer);
        }


        public IActionResult BuyNow(Guid productId)
        {
            var user = _userManager.GetUserAsync(User).Result;
            if (user == null)
                return Redirect("/Account/Login");

            // 1️⃣ Kiểm tra sản phẩm
            var product = _context.Products.FirstOrDefault(p => p.Id == productId);
            if (product == null || product.Quantity <= 0)
            {
                TempData["Error"] = "Sản phẩm đã hết hàng!";
                return RedirectToAction("Index", "Product");
            }

            // 2️⃣ BỎ CHỌN tất cả cart cũ
            var oldCarts = _context.Carts
                .Where(c => c.UserId == user.Id);

            foreach (var c in oldCarts)
                c.IsSelected = false;

            // 3️⃣ Kiểm tra cart đã tồn tại chưa
            var cart = _context.Carts.FirstOrDefault(c =>
                c.UserId == user.Id && c.ProductId == productId);

            if (cart != null)
            {
                // Đã có → reset số lượng
                cart.Quantity = 1;
                cart.IsSelected = true;
            }
            else
            {
                // Chưa có → tạo mới
                cart = new Cart
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    ProductId = productId,
                    Quantity = 1,
                    IsSelected = true,
                    CreateAt = DateOnly.FromDateTime(DateTime.Now)
                };

                _context.Carts.Add(cart);
            }

            _context.SaveChanges();

            // 4️⃣ Sang trang thanh toán
            return RedirectToAction("Index", "Checkout");
        }


    }
}
