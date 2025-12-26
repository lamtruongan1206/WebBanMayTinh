using System;
using System.Collections.Generic;

namespace WebBanMayTinh.Models;

public partial class Cart
{
    public Guid Id { get; set; }

    public int? Quantity { get; set; }

    public DateOnly? CreateAt { get; set; }

    public Guid? ProductId { get; set; }

    public string? UserId { get; set; }

    public virtual Product? Product { get; set; }

    public virtual AppUser? User { get; set; }
}
