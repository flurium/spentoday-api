using Data.Models.ProductTables;
using Data.Models.ShopTables;
using Data.Models.UserTables;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Data.Common;

namespace Data;

public class Db : IdentityDbContext<User>
{
    public Db(DbContextOptions options) : base(options)
    {
    }

    public DbSet<Shop> Shops { get; set; } = default!;
    public DbSet<ShopDomain> ShopDomains { get; set; } = default!;
    public DbSet<ShopBanner> ShopBanners { get; set; } = default!;
    public DbSet<InfoPage> InfoPages { get; set; } = default!;
    public DbSet<SocialMediaLink> SocialMediaLinks { get; set; } = default!;
    public DbSet<Subscription> ShopSubscriptions { get; set; } = default!;

    public DbSet<Category> Categories { get; set; } = default!;
    public DbSet<ProductCategory> ProductCategories { get; set; } = default!;
    public DbSet<Product> Products { get; set; } = default!;
    public DbSet<ProductImage> ProductImages { get; set; } = default!;

    public DbSet<Order> Orders { get; set; } = default!;
    public DbSet<OrderProduct> OrderProducts { get; set; } = default!;

    public DbSet<Question> Questions { get; set; } = default!;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // User tables
        var user = builder.Entity<User>();
        user.HasKey(x => x.Id);

        var question = builder.Entity<Question>();
        question.HasKey(x => x.Id);

        // Shop tables
        var shop = builder.Entity<Shop>();
        shop.HasKey(x => x.Id);
        shop.HasOne(x => x.Owner).WithMany(x => x.Shops).HasForeignKey(x => x.OwnerId);

        var shopDomain = builder.Entity<ShopDomain>();
        shopDomain.HasKey(x => new { x.Domain, x.ShopId });
        shopDomain.HasOne(x => x.Shop).WithMany(x => x.Domains).HasForeignKey(x => x.ShopId);

        var shopBanner = builder.Entity<ShopBanner>();
        shopBanner.HasKey(x => x.Id);
        shopBanner.HasOne(x => x.Shop).WithMany(x => x.Banners).HasForeignKey(x => x.ShopId);

        var socialMediaLink = builder.Entity<SocialMediaLink>();
        socialMediaLink.HasKey(x => x.Id);
        socialMediaLink.HasOne(x => x.Shop).WithMany(x => x.SocialMediaLinks).HasForeignKey(x => x.ShopId);

        var infoPage = builder.Entity<InfoPage>();
        infoPage.HasKey(x => new { x.Slug, x.ShopId });
        infoPage.HasOne(x => x.Shop).WithMany(x => x.InfoPages).HasForeignKey(x => x.ShopId);
        infoPage.Property(x => x.Content).HasColumnType("text");

        var subscription = builder.Entity<Subscription>();
        subscription.HasKey(x => x.Id);
        subscription.HasOne(x => x.Shop).WithMany(x => x.Subscriptions).HasForeignKey(x => x.ShopId);

        // Product tables
        var product = builder.Entity<Product>();
        product.HasKey(x => x.Id);
        product.HasOne(x => x.Shop).WithMany(x => x.Products).HasForeignKey(x => x.ShopId);
        product.Property(x => x.SeoDescription).HasColumnType("text");

        var productImage = builder.Entity<ProductImage>();
        productImage.HasKey(x => x.Id);
        productImage.HasOne(x => x.Product).WithMany(x => x.Images).HasForeignKey(x => x.ProductId);

        var category = builder.Entity<Category>();
        category.HasKey(x => x.Id);
        category.HasOne(x => x.Shop).WithMany(x => x.Categories).HasForeignKey(x => x.ShopId);
        category.HasOne(x => x.Parent).WithMany(x => x.Subcategories).HasForeignKey(x => x.ParentId);

        var productCategory = builder.Entity<ProductCategory>();
        productCategory.HasKey(x => new { x.ProductId, x.CategoryId });
        productCategory.HasOne(x => x.Product).WithMany(x => x.ProductCategories).HasForeignKey(x => x.ProductId);
        productCategory.HasOne(x => x.Category).WithMany(x => x.ProductCategories).HasForeignKey(x => x.CategoryId);

        var order = builder.Entity<Order>();
        order.HasKey(x => x.Id);

        var orderProduct = builder.Entity<OrderProduct>();
        orderProduct.HasKey(x => x.Id);
        orderProduct.HasOne(x => x.Order).WithMany(x => x.OrderProducts).HasForeignKey(x => x.OrderId);
        orderProduct.HasOne(x => x.Product).WithMany(x => x.OrderProducts).HasForeignKey(x => x.ProductId);
    }
}