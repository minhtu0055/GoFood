using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoFood.Data.Entities
{
    public class PromotionProduct
    {
        public Guid PromotionId { get; set; }
        public Guid ProductId { get; set; }

        public Promotion Promotion { get; set; }
        public Products Product { get; set; }
    }
}
