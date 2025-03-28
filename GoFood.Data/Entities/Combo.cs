using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoFood.Data.Entities
{
    public class Combo
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public string ImagePath { get; set; }
        public bool IsAvailable { get; set; }
        public List<ComboProduct> ComboProducts { get; set; }
        public List<CartDetails> CartDetails { get; set; }
        public List<OrderDetails> OrderDetails { get; set; }
    }
}
