using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WebBanMayTinh.Models;

namespace WebBanMayTinh.Models;

public partial class DataContext : IdentityDbContext<AppUser>
{
    public DataContext()
    {
    }

    public DataContext(DbContextOptions<DataContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Bill> Bills { get; set; }

    public virtual DbSet<BillDetail> BillDetails { get; set; }

    public virtual DbSet<Cart> Carts { get; set; }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<Image> Images { get; set; }

    public virtual DbSet<Specification> Specifications { get; set; }

    public virtual DbSet<Order> Orders { get; set; }
    public virtual DbSet<OrderItems> OrderItems { get; set; }
    public virtual DbSet<Address> Addresses { get; set; }
    public virtual DbSet<PasswordOtp> PasswordOtps { get; set; }

    public DbSet<ProductImport> ProductImports { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer("Data Source=DESKTOP-UCKVMB6\\SQLEXPRESS;Initial Catalog=ShopMayTinh;Integrated Security=True;TrustServerCertificate=true;");
     //   => optionsBuilder.UseSqlServer("Data Source=DESKTOP-UVNBRQT\\SQLEXPRESS;Initial Catalog=ShopMayTinh;Integrated Security=True;TrustServerCertificate=true;");

public DbSet<WebBanMayTinh.Models.Brand> Brand { get; set; } = default!;
}
