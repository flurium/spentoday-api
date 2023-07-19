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
    public DbSet<Banner> Banners { get; set; }
    public DbSet<Image> Images { get; set; }
    public DbSet<SocialMediaLink> SocialMediaLinks { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        
        builder.Entity<Product>().HasKey(p => p.Id);
        builder.Entity<Product>().HasOne(p => p.Shop).WithMany(s => s.Products).HasForeignKey(p => p.ShopId);
        builder.Entity<Product>().Property(p=>p.SeoDescription).HasColumnType("text");

        builder.Entity<Shop>().HasKey(s=>s.Id);
      
        builder.Entity<Banner>().HasKey(b => b.Url);
        builder.Entity<Banner>().HasOne(b =>b.Shop).WithMany(s => s.Banners).HasForeignKey(b => b.ShopId);

        builder.Entity<Image>().HasKey(i => i.Url);
        builder.Entity<Image>().HasOne(i => i.Product).WithMany(p => p.Images).HasForeignKey(i => i.ProductId);

        builder.Entity<SocialMediaLink>().HasKey(s=>s.Id);
        builder.Entity<SocialMediaLink>().HasOne(s=>s.Shop).WithMany(S => S.SocialMediaLinks).HasForeignKey(s => s.ShopId);

    }
}