using System.ComponentModel.DataAnnotations;

namespace WebBanMayTinh.Models.Views
{
    public class CreateProductReviewVM
    {
        public Guid OrderId { get; set; }

        public List<ProductReviewItemVM> Reviews { get; set; } = new();
    }

    public class ProductReviewItemVM
    {
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string? ProductThumbnailUrl { get; set; }

        [Range(1, 5)]
        public int Rating { get; set; }

        public string? Comment { get; set; }
    }
}
