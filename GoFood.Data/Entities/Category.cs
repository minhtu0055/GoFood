using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.Core.Pipeline;

namespace GoFood.Data.Entities
{
    public class Category
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public List<Products> Products { get; set; }    

    }
}
