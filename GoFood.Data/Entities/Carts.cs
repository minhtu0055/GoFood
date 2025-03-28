using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoFood.Data.Entities
{
    public  class Carts
    {
        public Guid Id { get; set; }
        public string UserId { get; set; }
        public AppUsers User { get; set; }
        public List<CartDetails> CartDetails {  get; set; }
        
    }
}
