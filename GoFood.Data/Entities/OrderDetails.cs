using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoFood.Data.Entities
{
    public class OrderDetails
    {
        public Guid Id { get; set; }
        public Guid OrderId { get; set; }
        public Order? Order { get; set; }
        public Guid ProductId { get; set; }
        public Guid ComboId { get; set; }
        public Combo? Combo { get; set; }
        public Products? Products { get; set; }
        public int Quantity { get; set; }
        public decimal SellingPrice { get; set; }
    }
}
