using WebBanMayTinh.Models;

namespace WebBanMayTinh.Areas.Admin.Models.Views
{
    public class ProductImportVM
    {
        public Guid ProductId { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public List<ProductImport> ImportHistories { get; set; } = new();
    }
}
