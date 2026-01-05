using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using WebBanMayTinh.Models;
using WebBanMayTinh.Models.Views;
using WebBanMayTinh.Services;
using WebBanMayTinh.Utils;

namespace WebBanMayTinh.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private readonly DataContext _context;
        private readonly UserManager<AppUser> _userManage;
        private readonly IUserService _userService;
        private readonly IEmailSender _emailSender;

        public OrderController(DataContext context, UserManager<AppUser> userManage, IUserService userService, IEmailSender emailSender)
        {
            _context = context;
            _userManage = userManage;
            _userService = userService;
            _emailSender = emailSender;
        }

        private async Task<AppUser?> GetCurrentUser()
        {
            return await _userManage.GetUserAsync(User);
        }

        public async Task<IActionResult> Index(string? orderStatus, string? keyword,
            int? page = 1,
            int? pageSize = 3)
        {
            var user = await GetCurrentUser();

            IQueryable<Order> query = _context.Orders
                .Include(o => o.OrderItems).ThenInclude(oi => oi.Product)
                .Where(o => o.UserId == user.Id)
                .AsQueryable();

            if (!string.IsNullOrEmpty(orderStatus)
                    && Enum.TryParse<OrderStatus>(orderStatus, out var status))
            {
                query = query.Where(o => o.Status == status);
            }
            if (!string.IsNullOrEmpty(keyword))
            {
                var keywordId = Guid.TryParse(keyword, out var id);

                query = query.Where(o =>
                    o.OrderItems.Any(oi =>
                        oi.Product.Name.Contains(keyword)) || 
                    (id != null && o.Id == id));
            }

            ViewBag.Keyword = keyword;
            ViewBag.OrderStatus = orderStatus;

            var orderVMs = await query
                .OrderByDescending(o => o.CreatedAt)
                .Select(o => new OrderVM
                {
                    Id = o.Id,
                    OrderStatus = o.Status,
                    TotalAmount = o.TotalAmount,
                    Quantity = o.Quantity,
                    IsReviewed = o.IsReviewed,
                    IsCancelRequested = o.IsCancelRequested,
                    Items = o.OrderItems.Select(oi => new OrderItemVM
                    {
                        ProductId = oi.ProductId,
                        ProductThumbnailUrl = oi.Product.ThumbnailUrl,
                        ProductName = oi.Product.Name,
                        Price = oi.Price,
                        Quantity = oi.Quantity,
                    }).ToList(),
                })
                .ToPagedResultAsync(page.Value, pageSize.Value);

            return View(orderVMs);
        }

        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.Address)
                .Include(o => o.User)
                .Include(o => o.OrderStatusHistories)
                .Include(o => o.OrderItems).ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (order == null)
            {
                return NotFound();
            }
            var vm = new OrderDetailVM
            {
                Id = order.Id,
                Address = order.Address,
                User = order.User,
                AddressId = order.AddressId,
                UserId = order.User.Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Status = order.Status,
                ShippingFee = order.ShippingFee,
                OrderStatusHistories = order.OrderStatusHistories.OrderByDescending(osh => osh.UpdateTime).ToList(),
                Items = order.OrderItems
                    .Select(oi => new OrderItemVM
                        {
                            ProductId = oi.ProductId,
                            ProductName = oi.Product.Name!,
                            Price = oi.Price,
                            Quantity = oi.Quantity,
                        }).ToList()
            };

            return View(vm);
        }

        public IActionResult Create()
        {
            ViewData["AddressId"] = new SelectList(_context.Addresses, "Id", "AddressLine");
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,CreatedAt,UpdatedAt,Status,Subtotal,ShippingFee,TotalAmount,UserId,AddressId")] Order order)
        {
            if (ModelState.IsValid)
            {
                order.Id = Guid.NewGuid();
                _context.Add(order);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["AddressId"] = new SelectList(_context.Addresses, "Id", "AddressLine", order.AddressId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", order.UserId);
            return View(order);
        }

        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }
            ViewData["AddressId"] = new SelectList(_context.Addresses, "Id", "AddressLine", order.AddressId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", order.UserId);
            return View(order);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,CreatedAt,UpdatedAt,Status,Subtotal,ShippingFee,TotalAmount,UserId,AddressId")] Order order)
        {
            if (id != order.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(order);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OrderExists(order.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["AddressId"] = new SelectList(_context.Addresses, "Id", "AddressLine", order.AddressId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", order.UserId);
            return View(order);
        }

        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.Address)
                .Include(o => o.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order != null)
            {
                _context.Orders.Remove(order);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool OrderExists(Guid id)
        {
            return _context.Orders.Any(e => e.Id == id);
        }

        [HttpGet]
        public async Task<IActionResult> CancelOrderRequest(Guid orderId)
        {
            var order = await _context.Orders.FindAsync(orderId);

            if (order == null)
                return NotFound();

            // Không cho yêu cầu hủy nếu đã giao hoặc đã hủy
            if (order.Status == OrderStatus.Completed || order.Status == OrderStatus.Cancelled && !order.IsCancelRequested)
                return BadRequest("Không thể hủy đơn hàng này");
            
            var vm = new CancelRequestVM
            {
                OrderId = order.Id
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelOrderRequest(CancelRequestVM vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            try
            {
                var order = await _context.Orders.FindAsync(vm.OrderId);

                if (order == null)
                    return NotFound();

                order.CancelReason = vm.CancelReason;
                order.IsCancelRequested = true;
                order.CancelRequestedAt = DateTime.Now;
                //order.Status = OrderStatus.CancelRequested;

                _context.Update(order);

                TempData["Success"] = "Đã gửi yêu cầu hủy đơn hàng";
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Không thể gửi yêu cầu hủy đơn hàng, vui lòng liên hệ chủ cửa hàng";
            }


            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmReceived(Guid id)
        {
            try
            {
                var order = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.Address)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                    .FirstOrDefaultAsync(o => o.Id == id);

                if (order == null)
                    return NotFound();

                order.Status = OrderStatus.Completed;

                var logoPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/assets/img/logo.png");

                var orderStatusHistory = new OrderStatusHistory
                {
                    OrderId = id,
                    Id = Guid.NewGuid(),
                    UpdateTime = DateTime.Now,
                    OrderStatus = order.Status
                };

                _context.Add(orderStatusHistory);
                _context.Update(order);

                var invoice = new Invoice
                {
                    OrderId = id,
                    CreateAt = DateTime.Now,
                    PaymentMethod = order.PaymentMethod,
                    Id = Guid.NewGuid(),
                };

                await _context.Invoices.AddAsync(invoice);
                await _context.SaveChangesAsync();

                var pdf = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(30);
                        page.DefaultTextStyle(x => x.FontSize(11));

                        // ===== HEADER =====
                        page.Header().Row(row =>
                        {
                            row.ConstantItem(120)
                               .Height(60)
                               .AlignMiddle()
                               .Image(logoPath, QuestPDF.Infrastructure.ImageScaling.FitArea);

                            row.RelativeItem().AlignRight().Column(col =>
                            {
                                col.Item().Text("HÓA ĐƠN BÁN HÀNG")
                                    .FontSize(20)
                                    .Bold();

                                col.Item().Text($"Số hóa đơn: {order.Id}");
                                col.Item().Text($"Ngày lập: {DateTime.Now:dd/MM/yyyy HH:mm}");
                            });
                        });

                        // ===== CONTENT =====
                        page.Content().PaddingVertical(10).Column(col =>
                        {
                            col.Spacing(15);

                            // --- Người bán & khách ---
                            col.Item().Row(row =>
                            {
                                row.RelativeItem().Column(c =>
                                {
                                    c.Item().Text("NGƯỜI BÁN").Bold();
                                    c.Item().Text("Công ty: WEB BÁN MÁY TÍNH");
                                    c.Item().Text("Hotline: 0123 456 789");
                                    c.Item().Text("Email: support@webbanmaytinh.vn");
                                });

                                row.RelativeItem().Column(c =>
                                {
                                    c.Item().Text("KHÁCH HÀNG").Bold();
                                    c.Item().Text($"{order.User.FirstName} {order.User.LastName}");
                                    c.Item().Text(order.Address.GetAddressDetail());
                                    c.Item().Text($"Thanh toán: {order.PaymentMethod}");
                                });
                            });

                            col.Item().LineHorizontal(1);

                            col.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.ConstantColumn(30);      // #
                                    columns.RelativeColumn(4);       // Sản phẩm
                                    columns.RelativeColumn(2);       // Đơn giá
                                    columns.RelativeColumn(1);       // SL
                                    columns.RelativeColumn(2);       // Thành tiền
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Element(CellStyle).Text("#").Bold();
                                    header.Cell().Element(CellStyle).Text("Sản phẩm").Bold();
                                    header.Cell().Element(CellStyle).AlignRight().Text("Đơn giá").Bold();
                                    header.Cell().Element(CellStyle).AlignCenter().Text("SL").Bold();
                                    header.Cell().Element(CellStyle).AlignRight().Text("Thành tiền").Bold();
                                });

                                int index = 1;
                                foreach (var item in order.OrderItems)
                                {
                                    table.Cell().Element(CellStyle)
                                        .Text(index++.ToString());

                                    table.Cell().Element(CellStyle)
                                        .Text(item.Product.Name);

                                    table.Cell().Element(CellStyle)
                                        .AlignRight()
                                        .Text(item.Price.ToString("N0"));

                                    table.Cell().Element(CellStyle)
                                        .AlignCenter()
                                        .Text(item.Quantity.ToString());

                                    table.Cell().Element(CellStyle)
                                        .AlignRight()
                                        .Text((item.Price * item.Quantity).ToString("N0"));
                                }
                            });

                            // --- Tổng tiền ---
                            col.Item().AlignLeft().Column(total =>
                            {
                                total.Item().Text($"Tạm tính: {order.Subtotal:N0} đ");
                                total.Item().Text($"Phí ship: {order.ShippingFee:N0} đ");
                                total.Item().Text($"TỔNG THANH TOÁN: {order.TotalAmount:N0} đ")
                                    .Bold()
                                    .FontSize(14);
                            });

                            col.Item().LineHorizontal(1);

                            // --- Người lập hóa đơn ---
                            //col.Item().Row(row =>
                            //{
                            //    row.RelativeItem().Column(c =>
                            //    {
                            //        c.Item().Text("Người lập hóa đơn").Bold();
                            //        c.Item().Text($"{staff.FirstName} {staff.LastName}");
                            //        c.Item().Text($"{DateTime.Now:dd/MM/yyyy HH:mm}");
                            //    });

                            //    row.RelativeItem().AlignRight().Column(c =>
                            //    {
                            //        c.Item().Text("Ký tên").Bold();
                            //        c.Item().PaddingTop(30);
                            //        c.Item().Text("______________________");
                            //    });
                            //});
                        });

                        page.Footer()
                            .AlignCenter()
                            .Text("Cảm ơn quý khách đã tin tưởng và mua hàng!");
                    });
                });

                var stream = new MemoryStream();
                pdf.GeneratePdf(stream);
                var pdfBytes = stream.ToArray();
                stream.Position = 0;

                await _emailSender.SendEmailWithAttachmentAsync(
                    toEmail: order.User.Email,
                    subject: "Hóa đơn mua hàng",
                    body: $@"
                        <p>Xin chào <b>{order.User.FirstName} {order.User.LastName}</b>,</p>
                        <p>Cảm ơn bạn đã mua hàng tại <b>Web Bán Máy Tính</b>.</p>
                        <p>Hóa đơn của bạn được đính kèm trong email này.</p>
                        <p>Trân trọng.</p>
                    ",
                    fileBytes: pdfBytes,
                    fileName: $"HoaDon_{order.Id}.pdf"
                );

            }
            catch (Exception ex)
            {
                TempData["Error"] = "Lỗi hệ thống, vui lòng thử lại";
            }
            return RedirectToAction("Index");
        }
        static IContainer CellStyle(IContainer container)
        {
            return container
                .PaddingVertical(6)
                .PaddingHorizontal(4)
                .AlignMiddle();
        }


    }
}
