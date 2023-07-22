using Data.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Data;

public class Db : IdentityDbContext<User>
{
    public Db(DbContextOptions options) : base(options)
    {
    }

    public DbSet<Category> Categories { get; set; } = default!;
    public DbSet<ProductCategory> ProductCategories { get; set; } = default!;
    public DbSet<Product> Products { get; set; } = default!;
    public DbSet<Shop> Shops { get; set; } = default!;
    public DbSet<ShopBanner> Banners { get; set; } = default!;
    public DbSet<ProductImage> Images { get; set; } = default!;
    public DbSet<SocialMediaLink> SocialMediaLinks { get; set; } = default!;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        var user = builder.Entity<User>();
        user.HasKey(x => x.Id);

        var product = builder.Entity<Product>();
        product.HasKey(p => p.Id);
        product.HasOne(p => p.Shop).WithMany(s => s.Products).HasForeignKey(p => p.ShopId);
        product.Property(p => p.SeoDescription).HasColumnType("text");

        var shop = builder.Entity<Shop>();
        shop.HasKey(s => s.Id);
        shop.HasOne(x => x.Owner).WithMany(x => x.Shops).HasForeignKey(x => x.OwnerId);

        var shopBanner = builder.Entity<ShopBanner>();
        shopBanner.HasKey(b => b.Url);
        shopBanner.HasOne(b => b.Shop).WithMany(s => s.Banners).HasForeignKey(b => b.ShopId);

        var productImage = builder.Entity<ProductImage>();
        productImage.HasKey(i => i.Url);
        productImage.HasOne(i => i.Product).WithMany(p => p.Images).HasForeignKey(i => i.ProductId);

        var category = builder.Entity<Category>();
        category.HasKey(c => c.Id);

        var productCategory = builder.Entity<ProductCategory>();
        productCategory.HasKey(pc => new { pc.ProductId, pc.CategoryId });
        productCategory.HasOne(pc => pc.Product).WithMany(p => p.ProductCategories).HasForeignKey(pc => pc.ProductId);
        productCategory.HasOne(pc => pc.Category).WithMany(c => c.ProductCategories).HasForeignKey(pc => pc.CategoryId);

        var socialMediaLink = builder.Entity<SocialMediaLink>();
        socialMediaLink.HasKey(s => s.Id);
        socialMediaLink.HasOne(s => s.Shop).WithMany(S => S.SocialMediaLinks).HasForeignKey(s => s.ShopId);
    }
}