using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyShop.Data.Entities
{
    public class Product
    {
        public int ProductId { get; set; }
        public required string SKU { get; set; }
        public required string Name { get; set; }
        public int ImportPrice { get; set; }
        public int Count { get; set; }
        public string? Description { get; set; }
        public int CategoryId { get; set; }
        public required Category Category { get; set; }
    }
}
