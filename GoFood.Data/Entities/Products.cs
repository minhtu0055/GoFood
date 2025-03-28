using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.Core.Pipeline;
using GoFood.Data.Enums;

namespace GoFood.Data.Entities
{
    public class Products
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public DateTime DateCreated { get; set; }
        public bool IsAvailable { get; set; }
        public Guid CategoryId { get; set; }
        public Category Category { get; set; }
        public ProductStatus Status { get; set; }
        public List<ProductImage> ProductImage { get; set; }
        public List<OrderDetails> OrderDetails { get; set; }
        public List<CartDetails> CartDetails { get; set; }
        public List<ComboProduct> ComboProducts { get; set; }
        public List<PromotionProduct> PromotionProducts { get; set; }
    }
}
