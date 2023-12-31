﻿using Data;
using Data.Models.ProductTables;
using Lib.EntityFrameworkCore;
using Lib.Storage;
using LinqKit;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers.ShopRoutes;

[Route("v1/shop/single")]
[ApiController]
public class SingleProductController : ControllerBase
{
    private readonly Db db;
    private readonly IStorage storj;

    public SingleProductController(Db db, IStorage storj)
    {
        this.db = db;
        this.storj = storj;
    }

    public record ProductOutput(
        string Id, string Name, double Price, double DiscountPrice, bool IsDiscount, int Amount,
        string SeoTitle, string SeoDescription, string SeoSlug,
        string Description, List<string> Images,
        IEnumerable<ProductProperty> Properties
    );

    public record struct ProductItemOutput(
        string Id, string Name, double Price, double DiscountPrice, bool IsDiscount,
        StorageFile? Image, string Slug
    );

    public record struct ProductCategoryOutput(string Id, string Name);

    public record struct ProductProperty(string Key, string Value);

    public record Output(
        ProductOutput Product, List<ProductItemOutput> Products,
        List<ProductCategoryOutput> Categories
    );

    [HttpGet("{domain}/{slugOrId}/product")]
    public async Task<IActionResult> SingleProduct(
        [FromRoute] string domain, [FromRoute] string slugOrId
    )
    {
        var product = await db.Products
            .WithDomain(domain)
            .Where(x => x.IsDraft == false && (x.SeoSlug == slugOrId || x.Id == slugOrId))
            .Select(x => new
            {
                x.Id,
                x.Name,
                x.Price,
                x.DiscountPrice,
                x.IsDiscount,
                x.Amount,
                x.SeoTitle,
                x.SeoDescription,
                x.SeoSlug,
                x.Description,
                Properties = x.Properties.Select(p => new ProductProperty(p.Key, p.Value))
            })
            .QueryOne();
        if (product == null) return NotFound();

        var images = await db.ProductImages.QueryMany(x => x.ProductId == product.Id);
        var productOutput = new ProductOutput(
            product.Id, product.Name, product.Price, product.DiscountPrice,
            product.IsDiscount, product.Amount, product.SeoTitle,
            product.SeoDescription, product.SeoSlug, product.Description,
            images.Select(x => storj.Url(x.GetStorageFile())).ToList(),
            product.Properties
        );

        var similar = await SimilarProducts(domain, product.Name);

        var categories = await db.ProductCategories
            .Where(x => x.ProductId == product.Id)
            .OrderBy(x => x.Order)
            .Select(x => new ProductCategoryOutput(x.CategoryId, x.Category.Name))
            .QueryMany();

        var output = new Output(productOutput, similar, categories);
        return Ok(output);
    }

    [NonAction]
    public async Task<List<ProductItemOutput>> SimilarProducts(string domain, string name)
    {
        var keywords = name.Split(' ').Select(x => x.ToLower()).ToList();

        var keywordsPredicate = PredicateBuilder.New<Product>();
        foreach (var keyword in keywords)
        {
            keywordsPredicate = keywordsPredicate.Or(p => p.Name.ToLower().Contains(keyword));
        }

        var products = await db.Products
            .WithDomain(domain)
            .Where(x => x.IsDraft == false && x.Name != name)
            .Where(keywordsPredicate)
            .Select(x => new ProductItemOutput(
                x.Id, x.Name, x.Price, x.DiscountPrice, x.IsDiscount,
                x.Images.OrderBy(i => i.Id == x.PreviewImage).Select(x => x.GetStorageFile()).FirstOrDefault(),
                x.SeoSlug
            ))
            .Take(5)
            .QueryMany();

        return products;
    }
}