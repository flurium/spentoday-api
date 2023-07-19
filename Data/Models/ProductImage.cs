
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Models
{
    public class ProductImage
    {
        public string Url { get ; set; }
        public string ProductId { get; set; }
        public Product Product { get; set; }

        public ProductImage() { }

        public ProductImage(string Url, string ProductId) {
            this.Url = Url;
            this.ProductId = ProductId;
        }

    }

}
