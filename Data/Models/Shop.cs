using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace Data.Models
{
    public class Shop
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; }
        public string LogoUrl { get; set; }
        public IReadOnlyCollection<SocialMediaLink> SocialMediaLinks { get; set; }
        public IReadOnlyCollection<Product> Products { get; set; }
        public IReadOnlyCollection<ShopBanner> Banners { get; set; }

        public Shop(string Name, string LogoUrl)
        {
            this.Name = Name;
            this.LogoUrl = LogoUrl;
        }
    }
}
