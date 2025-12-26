using System;
using System.Collections.Generic;

namespace WebBanMayTinh.Models;

public partial class BillDetail
{
    public Guid Id { get; set; }
    public Guid BillId { get; set; }

    public Guid ProductId { get; set; }

    public int? Quantity { get; set; }

    public decimal? UnitPrice { get; set; }

    public virtual Product Product { get; set; } = null!;
    public virtual Bill Bill { get; set; } = null!;
}
