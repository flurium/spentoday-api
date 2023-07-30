using Data;
using Data.Models.ProductTables;
using Backend.Services;
using Lib.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Lib.Storage;
using Lib.Storage.Services;
using System.Drawing;
using System.Drawing.Imaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Lib;

namespace Backend.Controllers
{
    [Route("v1/products")]
    [ApiController]
    [Authorize]
    public class ProductController : ControllerBase
    {
        private readonly Db db;

        private readonly ImageService imageService;

        private readonly IStorage storage;

        private readonly Jwt jwt;


        public ProductController(Db context, ImageService imageService, IStorage storage, Jwt jwt)
        {
            db = context;
            this.imageService = imageService;
            this.storage = storage;
            this.jwt = jwt;
        }


        [HttpPost]
        public async Task<IActionResult> CreateProduct(CreateProductInput newProduct)
        {
            var uid = User.FindFirst(Jwt.Uid)?.Value;
            var shop = await db.Shops.QueryOne(s => s.Id == newProduct.ShopId && s.OwnerId == uid);
           
            if (shop == null) return Forbid();

            Product product = new Product();

            product.IsDraft = true;
            product.Name = newProduct.Name;
            product.SeoSlug = newProduct.SeoSlug;
            product.ShopId = newProduct.ShopId;

            db.Products.Add(product);

            var saved = await db.Save();

            return saved ? Ok() : Problem();
        }

    
        [HttpGet("{id}")]
        public async Task<ActionResult<Product>> GetProduct(string id)
        {
            var product = await db.Products.QueryOne(p => p.Id == id);

            if (product == null) return Problem();

            var uid = User.FindFirst(Jwt.Uid)?.Value;
            var shop = await db.Shops.QueryOne(s => s.Id == product.ShopId && s.OwnerId == uid);

            if (shop == null) return Forbid();
           
            return Ok(product);
        }

   
        [HttpPatch("{id}")]
        public async Task<IActionResult> UpdateProduct([FromBody] UpdateProductInput patchDoc)
        {
            var product = await db.Products.QueryOne(p => p.Id == patchDoc.Id);

            if (product == null) return Problem();

            var uid = User.FindFirst(Jwt.Uid)?.Value;
            var shop = await db.Shops.QueryOne(s => s.Id == product.ShopId && s.OwnerId == uid);

            if (product == null)  return NotFound();

            if (patchDoc.Name != null) product.Name = patchDoc.Name;
            

            if (patchDoc.Amount != 0) product.Amount = patchDoc.Amount;
            

            if (patchDoc.Price != 0) product.Price = patchDoc.Price;
            

            if (patchDoc.PreviewImage != null) product.PreviewImage = patchDoc.PreviewImage;
            

            if (patchDoc.VideoUrl != null) product.VideoUrl = patchDoc.VideoUrl;
          

            if (patchDoc.SeoTitle != null) product.SeoTitle = patchDoc.SeoTitle;
    

            if (patchDoc.SeoSlug != null) product.SeoSlug = patchDoc.SeoSlug;
           

            if (patchDoc.SeoDescription != null) product.SeoDescription = patchDoc.SeoDescription;
            

            var saved = await db.Save();

            return saved ? Ok(product) : Problem();
        }


        [HttpPut("{id}/publish")]
        public async Task<IActionResult> PublishProduct(string id)
        {
            var product = await db.Products.QueryOne(p => p.Id == id);

            if (product == null) return Problem();

            var uid = User.FindFirst(Jwt.Uid)?.Value;
            var shop = await db.Shops.QueryOne(s => s.Id == product.ShopId && s.OwnerId == uid);
           
            if (shop == null) return Forbid();

            product.IsDraft = false;
            var saved = await db.Save();

            return saved ? Ok() : Problem();
        }


        [HttpPost("{id}/image")]
        public async Task<IActionResult> UploadProductImage(string id, [FromForm] IFormFile imageFile)
        {
                var product = await db.Products.QueryOne(p => p.Id == id);

                if (product == null) return Problem();

                var uid = User.FindFirst(Jwt.Uid)?.Value;
                var shop = await db.Shops.QueryOne(s => s.Id == product.ShopId && s.OwnerId == uid);

                if (shop == null) return Forbid();

                if (!IsImageFile(imageFile)) return BadRequest();

                var fileId = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);

                using (var stream = imageFile.OpenReadStream())
                {
                   var uploadedFile = await storage.Upload(fileId, stream);

                   if (uploadedFile == null) return Problem();
                   
                   var productImage = new ProductImage(uploadedFile.Provider, uploadedFile.Bucket, uploadedFile.Key, id);
                   db.ProductImages.Add(productImage);
                   
                }

                var saved = await db.Save();
                return saved ? Ok() : Problem();
        }

        [NonAction]
        public static bool IsImageFile(IFormFile file)
        {
            if (file == null || string.IsNullOrEmpty(file.FileName) || file.Length == 0)
            {
                return false;
            }

            var fileExtension = Path.GetExtension(file.FileName).ToLower();
            string[] photoExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".svg", ".webp", ".tiff", ".ico", ".jfif", ".psd", ".eps", ".pict", ".pic", "pct" }; ;

            return photoExtensions.Contains(fileExtension);
        }

        [HttpDelete("{id}/image")]
        public async Task<IActionResult> DeleteProductImage(string fileId)
        {
            var productImagesToDelete = await db.ProductImages.QueryOne(pi => pi.Id == fileId);

            var uid = User.FindFirst(Jwt.Uid)?.Value;
            var shop = await db.Shops.QueryOne(s => s.Id == productImagesToDelete.Product.ShopId && s.OwnerId == uid);

            if (shop == null) return Forbid();

            bool isDeleted = await storage.Delete(productImagesToDelete);

            if (!isDeleted) return Problem();
            
            var saved = await db.Save();
            return saved ? Ok() : Problem();
            
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(string id)
        {
            var product = await db.Products.QueryOne(p => p.Id == id);

            if (product == null) return Problem();

            var uid = User.FindFirst(Jwt.Uid)?.Value;
            var shop = await db.Shops.QueryOne(s => s.Id == product.ShopId && s.OwnerId == uid);

            if (shop == null) return Forbid();


            bool canDeleteProduct = !await db.Orders.AnyAsync(o => o.ProductId == id);

            if (canDeleteProduct)
            {
                var images = await db.ProductImages.QueryMany(x => x.ProductId == product.Id);
                await imageService.SafeDelete(images);
                db.Products.Remove(product);
            } else{
                product.IsArchive = true;
            }

            var saved = await db.Save();
            return saved ? Ok() : Problem();
        }
    }

    public class CreateProductInput
    {
        public string Name { get; set; }
        public string SeoSlug { get; set; } = string.Empty;
        public string ShopId { get; set; }
    }

    public class UpdateProductInput
    {
        public string Id { get; set; }
        public string Name { get; set; } = null;
        public double Price { get; set; } = 0;
        public int Amount { get; set; } = 0;
        public string PreviewImage { get; set; } = null;
        public string VideoUrl { get; set; } = null;
        public string SeoTitle { get; set; } = null;
        public string SeoDescription { get; set; } = null;
        public string SeoSlug { get; set; } = null;
    }
}
