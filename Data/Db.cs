using Data.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Data;

public class Db : IdentityDbContext<User>
{
    public Db(DbContextOptions options) : base(options)
    {
        Database.EnsureCreated();
    }

    public DbSet<Product> Products { get; set; }
    public DbSet<Shop> Shops { get; set; }
    public DbSet<ShopBanner> Banners { get; set; }
    public DbSet<ProductImage> Images { get; set; }
    public DbSet<SocialMediaLink> SocialMediaLinks { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Product>().HasKey(p => p.Id);
        builder.Entity<Product>().HasOne(p => p.Shop).WithMany(s => s.Products).HasForeignKey(p => p.ShopId);
        builder.Entity<Product>().Property(p => p.SeoDescription).HasColumnType("text");

        builder.Entity<Shop>().HasKey(s => s.Id);

        builder.Entity<ShopBanner>().HasKey(b => b.Url);
        builder.Entity<ShopBanner>().HasOne(b => b.Shop).WithMany(s => s.Banners).HasForeignKey(b => b.ShopId);

        builder.Entity<ProductImage>().HasKey(i => i.Url);
        builder.Entity<ProductImage>().HasOne(i => i.Product).WithMany(p => p.Images).HasForeignKey(i => i.ProductId);

        builder.Entity<SocialMediaLink>().HasKey(s => s.Id);
        builder.Entity<SocialMediaLink>().HasOne(s => s.Shop).WithMany(S => S.SocialMediaLinks).HasForeignKey(s => s.ShopId);
    }
}