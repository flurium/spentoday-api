using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Models
{
    public class SocialMediaLink
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Link { get; set; }
        public string ShopId { get; set; }
        public Shop Shop { get; set; }
    }
}
