using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoFood.ViewModels.Catalog.Category
{
    public class CategoryUpdateRequest
    {
        public Guid Id { get; set; }
        public string Name { get; set; }

    }
}
