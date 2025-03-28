using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GoFood.Data.Enums;

namespace GoFood.ViewModels.Catalog.Category
{
    public class CategoryViewModels
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public int ProductCount { get; set; }
    }
}
