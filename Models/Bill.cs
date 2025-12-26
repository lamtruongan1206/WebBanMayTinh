using System;
using System.Collections.Generic;

namespace WebBanMayTinh.Models;

public partial class Bill
{
    public Guid Id { get; set; }

    public string? UserId { get; set; }

    public DateOnly? CreateAt { get; set; }

    public string? PaymentMethod { get; set; }

    public decimal? TotalPrice { get; set; }

    public string? Status { get; set; }

    public virtual AppUser? User { get; set; }
}
