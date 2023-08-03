using Backend.Services;
using Data;
using Data.Models.ShopTables;
using Lib.EntityFrameworkCore;
using Lib;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Data.Models.ProductTables;
using Lib.Storage;
using Data.Models.UserTables;

namespace Backend.Controllers.ShopControllers
{
    [Route("v1/site/shopsettings")]
    [ApiController]
    public class ShopSettingsController : ControllerBase
    {
        private readonly Db db;
        private readonly ImageService imageService;

        private readonly IStorage storage;

        public ShopSettingsController(Db context, ImageService imageService, IStorage storage)
        {
            db = context;
            this.imageService = imageService;
            this.storage = storage;
        }
        public record LinkIn(string Name, string Link );
        public record LinkOut(string Name, string Link, string Id);
        public record BannerOut(string Url, string Id);
        public record ShopUpdate(string Name);
        [HttpPost("{shopId}/addlink")]
        [Authorize]
        public async Task<IActionResult> AddLink([FromBody] LinkIn link, [FromRoute]string shopId)
        {
            var uid = User.FindFirst(Jwt.Uid);
            if (uid == null) return Unauthorized();

            var shop = await db.Shops
            .QueryOne(x => x.Id == shopId && x.OwnerId == uid.Value);

            if (shop == null) return Problem();

            var newLink = new SocialMediaLink(link.Name, link.Link, shopId);

            await db.SocialMediaLinks.AddAsync(newLink);

            var saved = await db.Save();
            return saved ? Ok(new LinkOut(newLink.Name, newLink.Link, newLink.Id)) : Problem();
        }
        [HttpGet("{shopId}/getlinks")]
        [Authorize]
        public async Task<IActionResult> GetLinks([FromRoute] string shopId)
        {
            var uid = User.FindFirst(Jwt.Uid);
            if (uid == null) return Unauthorized();

            var shop = await db.Shops
           .QueryOne(x => x.Id == shopId && x.OwnerId == uid.Value);

            if (shop == null) return NotFound(); 

            var links = await db.SocialMediaLinks
            .Where(x => x.ShopId == shopId)
            .Select(x => new LinkOut(x.Name, x.Link,x.Id))
            .QueryMany();

            var saved = await db.Save();
            return saved ? Ok(links) : Problem();
        }
        [HttpDelete("{linkId}/deletelink")]
        [Authorize]
        public async Task<IActionResult> DeleteLink([FromRoute] string linkId)
        {
            var uid = User.FindFirst(Jwt.Uid);
            if (uid == null) return Unauthorized();

            var link = await db.SocialMediaLinks
           .Where(x => x.Id == linkId)
           .QueryOne();

            if (link == null) return NotFound();

            db.SocialMediaLinks.Remove(link);

            var saved = await db.Save();
            return saved ? Ok() : Problem();
        }
        [HttpPost("{shopId}/addbanner")]
        [Authorize]
        public async Task<IActionResult> AddBanner(IFormFile file, [FromRoute] string shopId)
        {
            var uid = User.FindFirst(Jwt.Uid);
            if (uid == null) return Unauthorized();

            var banner = file;
            if (!imageService.IsImageFile(banner)) return BadRequest();

            var shop = await db.Shops
            .QueryOne(x => x.Id == shopId && x.OwnerId == uid.Value);

            if (shop == null) return Problem();

            var fileId = Guid.NewGuid().ToString() + Path.GetExtension(banner.FileName);
            ShopBanner shopBanner;

            using (var stream = banner.OpenReadStream())
            {
                var uploadedFile = await storage.Upload(fileId, stream);

                if (uploadedFile == null) return Problem();

                shopBanner = new ShopBanner(uploadedFile.Provider, uploadedFile.Bucket, uploadedFile.Key, shopId);
                await db.ShopBanners.AddAsync(shopBanner);
            }

            var saved = await db.Save();

            if (!saved)
            {
                await imageService.SafeDeleteOne(shopBanner);
                return Problem();
            }
            return Ok(new BannerOut(storage.Url(shopBanner), shopBanner.Id));
        }
        [HttpDelete("{bannerId}/deletebanner")]
        [Authorize]
        public async Task<IActionResult> DeleteBanner( [FromRoute] string bannerId)
        {
            var uid = User.FindFirst(Jwt.Uid);
            if (uid == null) return Unauthorized();

            var banner = await db.ShopBanners
           .Where(x => x.Id == bannerId)
           .QueryOne();

            if (banner == null) return Problem();

            await imageService.SafeDeleteOne(banner);
            db.ShopBanners.Remove(banner);
            var saved = await db.Save();
            return saved ? Ok() : Problem();
        }
        [HttpGet("{shopId}/getbanners")]
        [Authorize]
        public async Task<IActionResult> GetBanners([FromRoute] string shopId)
        {
            var uid = User.FindFirst(Jwt.Uid);
            if (uid == null) return Unauthorized();

            var shop = await db.Shops
           .QueryOne(x => x.Id == shopId && x.OwnerId == uid.Value);

            if (shop == null) return NotFound();

            var banners = await db.ShopBanners
            .Where(x => x.ShopId == shopId)
            .Select(x => new BannerOut(storage.Url(x.GetStorageFile()), x.Id))
            .QueryMany();

            var saved = await db.Save();
            return saved ? Ok(banners) : Problem();
        }
        [HttpPost("{shopId}/name")]
        [Authorize]
        public async Task<IActionResult> UpdateShopName([FromRoute] string shopId , [FromBody] ShopUpdate shopName)
        {
            var uid = User.FindFirst(Jwt.Uid);
            if (uid == null) return Unauthorized();

            var shop = await db.Shops
           .QueryOne(x => x.Id == shopId && x.OwnerId == uid.Value);

            if (shop == null) return NotFound();

            shop.Name = shopName.Name;

            var saved = await db.Save();
            return saved ? Ok() : Problem();
        }
        [HttpPost("{shopId}/logo")]
        [Authorize]
        public async Task<IActionResult> UploadLogo([FromRoute] string shopId, IFormFile file)
        {
            var uid = User.FindFirst(Jwt.Uid);
            if (uid == null) return Unauthorized();

            var shop = await db.Shops
           .QueryOne(x => x.Id == shopId && x.OwnerId == uid.Value);

            if (shop == null) return Problem();

            if (imageService.IsImageFile(file))
            {
                var logo = shop.GetStorageFile();
                if (logo != null)
                {
                    await imageService.SafeDeleteOne(logo);
                }

                var fileId = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);

                using (var stream = file.OpenReadStream())
                {
                    var uploadedFile = await storage.Upload(fileId, stream);

                    if (uploadedFile == null) return Problem();

                    shop.LogoBucket = uploadedFile.Bucket;
                    shop.LogoKey = uploadedFile.Key;
                    shop.LogoProvider = uploadedFile.Provider;
                }
            }
            else return BadRequest();

            var saved = await db.Save();
            return saved ? Ok(storage.Url(shop.GetStorageFile())) : Problem();
        }
        [HttpGet("{shopId}/getname")]
        [Authorize]
        public async Task<IActionResult> GetName([FromRoute] string shopId)
        {
            var uid = User.FindFirst(Jwt.Uid);
            if (uid == null) return Unauthorized();

            var shop = await db.Shops
           .QueryOne(x => x.Id == shopId && x.OwnerId == uid.Value);

            if (shop == null) return NotFound();

            return  Ok(shop.Name);
        }
        [HttpGet("{shopId}/getlogo")]
        [Authorize]
        public async Task<IActionResult> GetLogo([FromRoute] string shopId)
        {
            var uid = User.FindFirst(Jwt.Uid);
            if (uid == null) return Unauthorized();

            var shop = await db.Shops
           .QueryOne(x => x.Id == shopId && x.OwnerId == uid.Value);

            if (shop == null) return Problem();
            var logo = shop.GetStorageFile();
            if (logo == null) return NotFound();

            return Ok(storage.Url(logo));
        }
    }

  

}
