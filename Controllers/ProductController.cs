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
        DataContext _context = new DataContext();


        // ================== INDEX ==================
        public IActionResult Index(
         string? searchName,
         string? searchManufacturer,
         decimal? priceFrom,
         decimal? priceTo,
         Guid? categoryId,     // lọc theo category
         int? brandId,         // lọc theo brand
         int page = 1)
        {
            int pageSize = 6;

            // ==============================
            // 1. Query gốc từ Product
            // ==============================
            var query = _context.Products
                .Include(p => p.Category)   // để hiển thị tên Category
                .Include(p => p.Images)     // để lấy ảnh
                .AsQueryable();

            // ==============================
            // 2. FILTER
            // ==============================
            if (!string.IsNullOrEmpty(searchName))
                query = query.Where(p => p.Name.Contains(searchName));

            if (!string.IsNullOrEmpty(searchManufacturer))
                query = query.Where(p => p.Manufacturer.Contains(searchManufacturer));

            if (priceFrom.HasValue)
                query = query.Where(p => p.Price >= priceFrom.Value);

            if (priceTo.HasValue)
                query = query.Where(p => p.Price <= priceTo.Value);

            if (categoryId.HasValue)
                query = query.Where(p => p.CategoryId == categoryId);

            if (brandId.HasValue)
                query = query.Where(p => p.BrandId == brandId);

            // ==============================
            // 3. PHÂN TRANG
            // ==============================
            int totalItems = query.Count();
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var products = query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // ==============================
            // 4. DATA CHO SIDEBAR
            // ==============================
            ViewBag.Categories = _context.Categories.ToList(); // danh mục
            ViewBag.Brands = _context.Brand.ToList();          // thương hiệu

            // ==============================
            // 5. LƯU GIÁ TRỊ ĐANG CHỌN
            // ==============================
            ViewBag.SelectedCategory = categoryId;
            ViewBag.SelectedBrand = brandId;

            // ==============================
            // 6. VIEWBAG CHO FORM + PAGING
            // ==============================
            ViewBag.SearchName = searchName;
            ViewBag.SearchManufacturer = searchManufacturer;
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
                return View(dto);
            }

            if (dto.CategoryId == null || !_context.Categories.Any(c => c.Id == dto.CategoryId))
            {
                ModelState.AddModelError("CategoriesId", "Danh mục không tồn tại");
                ViewBag.CategoriesId = new SelectList(_context.Categories, "Id", "Name", dto.CategoryId);
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
            ViewBag.ComputerId = computer.Id;
            ViewBag.MainImage = computer.Images?.FirstOrDefault(i => i.IsMain)?.Url ?? "/images/default.png";
            var dto = new ComputerDto
            {
                Name = computer.Name,
                Manufacturer = computer.Manufacturer,
                Price = computer.Price,
                Quantity = computer.Quantity,
                Description = computer.Description,
                CategoriesId = computer.CategoryId
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
            // Lấy máy tính theo id, bao gồm các ảnh
            var computer = _context.Products
                .Include(c => c.Images)
                .Include(c => c.Category) // Nếu muốn hiển thị tên danh mục
                .FirstOrDefault(c => c.Id == id);

            if (computer == null) return NotFound();

            // Truyền model vào view
            return View(computer);
        }


    }
}
