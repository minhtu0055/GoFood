using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace GoFood.Data.Entities
{
    public class CartDetails
    {
        public Guid Id { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal Subtotal => Quantity * Price;
        public Guid CartId { get; set; }
        public Carts Carts { get; set; }
        public Guid? ProductId {  get; set; }
        public Products Products { get; set; }
        public Guid? ComboId { get; set; }
        public Combo Combo { get; set; }
    }
}
