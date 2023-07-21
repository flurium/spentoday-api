using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Models
{
    public class Category
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; }
        public IReadOnlyCollection<ProductCategory> ProductCategories { get; set; }

        public Category( string name)
        {
            this.Name = name;
        }
    }
}
