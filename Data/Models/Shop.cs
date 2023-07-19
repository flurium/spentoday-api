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
        public int Id { get; set; }
        public string Name { get; set; }
        public string LogoUrl { get; set; }
        public List<SocialMediaLink> SocialMediaLinks { get; set; }
        public List<Product> Products { get; set; }
        public List<Banner> Banners { get; set; }
    }
}
