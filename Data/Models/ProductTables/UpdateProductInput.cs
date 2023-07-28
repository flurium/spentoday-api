using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Models.ProductTables
{
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
