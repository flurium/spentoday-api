using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Models
{
    public class Product
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public double Price { get; set; }
        public int Amount { get; set; }
        public string PreviewImage { get; set; }
        public List<Image> Images { get; set; }
        //public List<CategoryToProduct> Categories?
        public string ShopId { get; set; }
        public Shop Shop { get; set; }
        public string VideoURL { get; set; }
        //SEO
        public string SeoTitle { get; set; }
        public string SeoDescription { get; set; }
        public string SeoSlug { get; set; }


    }
}
