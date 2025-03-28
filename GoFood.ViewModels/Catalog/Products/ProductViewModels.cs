using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GoFood.Data.Enums;

namespace GoFood.ViewModels.Catalog.Products
{
    public class ProductViewModels
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public DateTime DateCreated { get; set; }
        public bool IsAvailable { get; set; }
        public ProductStatus Status { get; set; }
        public Guid CategoryId { get; set; }
        public string CategoryName { get; set; }
        public List<string> Images { get; set; }
    }
}
