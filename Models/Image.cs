using System;
using System.Collections.Generic;

namespace WebBanMayTinh.Models;

public partial class Image
{
    public Guid Id { get; set; }

    public string? Url { get; set; }

    public bool IsMain { get; set; }

    public int? OrderNumber { get; set; }

    public Guid? ProductId { get; set; }

    public virtual Product? Product { get; set; }
}
