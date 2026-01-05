using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using WebBanMayTinh.Models;
using WebBanMayTinh.Services;

namespace WebBanMayTinh.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class InvoiceController : Controller
    {
        private readonly DataContext _context;
        private readonly IUserService _userService;
        private readonly IEmailSender _emailSender;

        public InvoiceController(DataContext context, IUserService userService, IEmailSender emailSender)
        {
            _context = context;
            _userService = userService;
            _emailSender = emailSender;
        }

        // GET: Admin/Invoice
        public async Task<IActionResult> Index()
        {
            var dataContext = _context.Invoices.Include(i => i.Order);
            return View(await dataContext.ToListAsync());
        }

        // GET: Admin/Invoice/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var invoice = await _context.Invoices
                .Include(i => i.Order)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (invoice == null)
            {
                return NotFound();
            }

            return View(invoice);
        }

        [HttpGet]
        public async Task<IActionResult> ExportInvoice(Guid? id)
        {
            var staff = await _userService.GetCurrentUser();
            if (staff == null) return BadRequest();

            if (id == null) return NotFound();

            var order = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.Address)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return NotFound();

            var logoPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/assets/img/logo.png");

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
                           .Image(logoPath, ImageScaling.FitArea);

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
                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text("Người lập hóa đơn").Bold();
                                c.Item().Text($"{staff.FirstName} {staff.LastName}");
                                c.Item().Text($"{DateTime.Now:dd/MM/yyyy HH:mm}");
                            });

                            row.RelativeItem().AlignRight().Column(c =>
                            {
                                c.Item().Text("Ký tên").Bold();
                                c.Item().PaddingTop(30);
                                c.Item().Text("______________________");
                            });
                        });
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

            //await _emailSender.SendEmailWithAttachmentAsync(
            //    toEmail: order.User.Email,
            //    subject: "Hóa đơn mua hàng",
            //    body: $@"
            //        <p>Xin chào <b>{order.User.FirstName} {order.User.LastName}</b>,</p>
            //        <p>Cảm ơn bạn đã mua hàng tại <b>Web Bán Máy Tính</b>.</p>
            //        <p>Hóa đơn của bạn được đính kèm trong email này.</p>
            //        <p>Trân trọng.</p>
            //    ",
            //    fileBytes: pdfBytes,
            //    fileName: $"HoaDon_{order.Id}.pdf"
            //);

            return File(stream, "application/pdf", $"HoaDon_{order.Id}.pdf");
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
