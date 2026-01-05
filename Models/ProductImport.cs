namespace WebBanMayTinh.Models
{
    public class ProductImport
    {
        public Guid Id { get; set; }            // Mã nhập kho
        public Guid ProductId { get; set; }     // Id máy tính nhập
        public int Quantity { get; set; }       // Số lượng nhập
        public DateTime ImportDate { get; set; }// Ngày nhập

        public Product Product { get; set; }
    }
}
