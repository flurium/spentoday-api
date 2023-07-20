using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Models
{
    public class ShopBanner
    {
        public string Url { get; set; }
        public string ShopId { get; set; }
        public Shop Shop { get; set; }

        public ShopBanner(string Url, string ShopId)
        {
            this.Url = Url;
            this.ShopId = ShopId;
        }
    }
}