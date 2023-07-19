using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Models
{
    public class Shop
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string LogoURL { get; set; }
        public List<SocialMediaLink> SocialMediaLinks { get; set; }
        public List<Product> Products { get; set; }
    }
}
