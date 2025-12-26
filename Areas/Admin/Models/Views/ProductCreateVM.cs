using WebBanMayTinh.Models;

namespace WebBanMayTinh.Areas.Admin.Models.Views
{
    public class ProductCreateVM
    {
        public string? Name { get; set; }

        public decimal? Price { get; set; }

        public int? Quantity { get; set; }

        public string? Description { get; set; }


        public Guid? CategoryId { get; set; }
        public virtual Category? Category { get; set; }


        public int? BrandId { get; set; }
        public virtual Brand? Brand { get; set; }


        public IFormFile? MainImage { get; set; }               // Ảnh chính
        public List<IFormFile>? AdditionalImages { get; set; }  // Ảnh phụ
    }
}
