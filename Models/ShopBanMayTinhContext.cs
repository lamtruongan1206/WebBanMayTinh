using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace WebBanMayTinh.Models;

public partial class ShopBanMayTinhContext : IdentityDbContext<AppUser>
{
    public ShopBanMayTinhContext()
    {
    }

    public ShopBanMayTinhContext(DbContextOptions<ShopBanMayTinhContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Bill> Bills { get; set; }

    public virtual DbSet<BillDetail> BillDetails { get; set; }

    public virtual DbSet<Cart> Carts { get; set; }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Computer> Computers { get; set; }

    public virtual DbSet<Image> Images { get; set; }

    //public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Specification> Specifications { get; set; }

    //public virtual DbSet<AppUser> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //=> optionsBuilder.UseSqlServer("Data Source=DESKTOP-UCKVMB6\\SQLEXPRESS;Initial Catalog=ShopBanMayTinh;Integrated Security=True;TrustServerCertificate=true;");
        => optionsBuilder.UseSqlServer("Data Source=DESKTOP-UVNBRQT\\SQLEXPRESS;Initial Catalog=ShopMayTinh;Integrated Security=True;TrustServerCertificate=true;");
}
