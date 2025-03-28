using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoFood.Data.Entities
{
    public class ComboProduct
    {
        public Guid ComboId { get; set; }
        public Combo Combo { get; set; }
        public Guid ProductId { get; set; }
        public Products Products { get; set; }
        public int Quantity { get; set; }
    }
}
