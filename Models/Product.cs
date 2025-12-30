using System;
using System.Collections.Generic;

namespace WebBanMayTinh.Models;

public partial class Product
{
    public Guid Id { get; set; }

    public string? Name { get; set; }

    public string? Manufacturer { get; set; }

    public decimal? Price { get; set; }

    public int? Quantity { get; set; }

    public DateOnly? UpdateAt { get; set; }

    public DateOnly? CreateAt { get; set; }


    public Guid? CategoryId { get; set; }
    public string? Description { get; set; }
    public int? BrandId { get; set; }
    public string? ThumbnailUrl { get; set; }

    public virtual Brand? Brand { get; set; }
    public virtual Category? Category { get; set; }

    public virtual ICollection<BillDetail> BillDetails { get; set; } = new List<BillDetail>();
    public virtual ICollection<Cart> Carts { get; set; } = new List<Cart>();
    public virtual ICollection<Image> Images { get; set; } = new List<Image>();
    public virtual ICollection<Specification> Specifications { get; set; } = new List<Specification>();
    public virtual ICollection<OrderItems> OrderItems { get; set; } = new List<OrderItems>();
}
