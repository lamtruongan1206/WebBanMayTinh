using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebBanMayTinh.Areas.Admin.Models.Views;
using WebBanMayTinh.Authorization;
using WebBanMayTinh.Models;
using WebBanMayTinh.Models.DTO;

namespace WebBanMayTinh.Areas.Controllers
{
    [Area("Admin")]
    [HasPermission(CustomClaimTypes.Permission, Permissions.ProductAccess)]
    public class ProductController : Controller
    {
        DataContext _context = new DataContext();

        [HasPermission(CustomClaimTypes.Permission, Permissions.ProductRead)]
        public IActionResult Index(string searchName, string searchBrand, decimal? priceFrom, decimal? priceTo, int page = 1, int pageSize = 6)
        {
            // Lấy dữ liệu cơ bản
            var query = _context.Products
                .AsNoTracking()
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Include(p => p.Images.Where(i => i.IsMain))
                .AsQueryable();

            // Lọc theo tên
            if (!string.IsNullOrEmpty(searchName))
            {
                query = query.Where(c => c.Name.Contains(searchName));
            }

            // Lọc theo brand
            if (!string.IsNullOrEmpty(searchBrand))
            {
                query = query.Where(c => c.Brand != null &&
                                         c.Brand.Name.Contains(searchBrand));
            }

            // Lọc theo khoảng giá
            if (priceFrom.HasValue)
                query = query.Where(c => c.Price >= priceFrom.Value);
            if (priceTo.HasValue)
                query = query.Where(c => c.Price <= priceTo.Value);

            // Tổng số bản ghi sau lọc
            int totalItems = query.Count();

            // Phân trang
            var computers = query
                .OrderBy(c => c.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // Truyền dữ liệu phân trang qua ViewBag
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            // Truyền lại các giá trị tìm kiếm để giữ trên form
            ViewBag.SearchName = searchName;
            ViewBag.SearchBrand = searchBrand;
            ViewBag.PriceFrom = priceFrom;
            ViewBag.PriceTo = priceTo;

            return View(computers);
        }

        [HttpGet]
        [HasPermission(CustomClaimTypes.Permission, Permissions.ProductCreate)]
        public IActionResult Add()
        {
            ViewBag.Categories = new SelectList(_context.Categories, "Id", "Name");
            ViewBag.BrandId = new SelectList(_context.Brand, "Id", "Name");
            return View(new ProductCreateVM());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [HasPermission(CustomClaimTypes.Permission, Permissions.ProductCreate)]
        public IActionResult Add(ProductCreateVM dto)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.CategoriesId = new SelectList(_context.Categories, "Id", "Name", dto.CategoryId);
                return View(dto);
            }
            if (ModelState.IsValid)
            {
                ViewBag.BrandId = new SelectList(_context.Brand, "Id", "Name", dto.BrandId);
                return View(dto);
            }

                if (dto.CategoryId == null || !_context.Categories.Any(c => c.Id == dto.CategoryId))
            {
                ModelState.AddModelError("CategoriesId", "Danh mục không tồn tại");
                ViewBag.CategoriesId = new SelectList(_context.Categories, "Id", "Name", dto.CategoryId);
                return View(dto);
            }
                if(dto.BrandId == null || !_context.Brand.Any(b => b.Id == dto.BrandId))
            {
                ModelState.AddModelError("BrandId", "Thương hiệu không tồn tại");
                ViewBag.BrandId = new SelectList(_context.Brand, "Id", "Name", dto.BrandId);
                return View(dto);
            }

            // Tạo entity Computer
            var product = new Product
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

            _context.Products.Add(product);

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

                product.ThumbnailUrl = mainPath;

                _context.Images.Add(new Image
                {
                    Id = Guid.NewGuid(),
                    ProductId = product.Id,
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
                        ProductId = product.Id,
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
        [HasPermission(CustomClaimTypes.Permission, Permissions.ProductUpdate)]
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
        [HasPermission(CustomClaimTypes.Permission, Permissions.ProductUpdate)]
        public IActionResult UpdateAction(Guid id, ComputerDto dto)
        {
            var computer = _context.Products
                .Include(c => c.Images)
                .FirstOrDefault(c => c.Id == id);

            if (computer == null) return NotFound();

            if (!ModelState.IsValid)
            {
                ViewBag.CategoriesId = new SelectList(_context.Categories, "Id", "Name", dto.CategoriesId);
                ViewBag.BrandId = new SelectList(_context.Brand, "Id", "Name", dto.BrandId);
                ViewBag.MainImage = computer.Images?.FirstOrDefault(i => i.IsMain)?.Url ?? "/images/default.png";
                return View("Update", dto);
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

                computer.ThumbnailUrl = fileName;

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
        [HasPermission(CustomClaimTypes.Permission, Permissions.ProductDelete)]
        public IActionResult Delete(Guid id)
        {
            var computer = _context.Products.FirstOrDefault(c => c.Id == id);
            if (computer == null) return NotFound();
            return View(computer);
        }

        [HttpPost]
        [HasPermission(CustomClaimTypes.Permission, Permissions.ProductDelete)]
        public IActionResult DeleteAction(Guid id)
        {
            var computer = _context.Products
                .Include(c => c.Images)
                .Include(c=> c.Brand)
                .Include(c => c.Category)
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
        [HasPermission(CustomClaimTypes.Permission, Permissions.ProductRead)]
        public IActionResult Detail(Guid id)
        {
            var computer = _context.Products
                .Include(c => c.Images)
                .Include(c => c.Category)
                .Include(c => c.Brand)
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

        [HttpGet]
        [HasPermission(CustomClaimTypes.Permission, Permissions.ProductUpdate)]
        public IActionResult Import(Guid productId)
        {
            var product = _context.Products.FirstOrDefault(p => p.Id == productId);
            if (product == null) return NotFound();

            var vm = new ProductImportVM
            {
                ProductId = product.Id,
                ProductName = product.Name
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [HasPermission(CustomClaimTypes.Permission, Permissions.ProductUpdate)]
        public IActionResult Import(ProductImportVM vm)
        {
            if (vm.Quantity <= 0)
            {
                ModelState.AddModelError("Quantity", "Số lượng nhập phải > 0");
                return View(vm);
            }

            var product = _context.Products.FirstOrDefault(p => p.Id == vm.ProductId);
            if (product == null) return NotFound();

            // 1️⃣ Cộng thêm số lượng
            product.Quantity += vm.Quantity;
            product.UpdateAt = DateOnly.FromDateTime(DateTime.Now);

            // 2️⃣ Lưu lịch sử nhập kho
            var import = new ProductImport
            {
                Id = Guid.NewGuid(),
                ProductId = product.Id,
                Quantity = vm.Quantity,
                ImportDate = DateTime.Now
            };

            _context.ProductImports.Add(import);
            _context.SaveChanges();

            TempData["Success"] = "Nhập hàng thành công";
            return RedirectToAction("Index");
        }
    }
}
