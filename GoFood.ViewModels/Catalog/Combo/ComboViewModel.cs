using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoFood.ViewModels.Catalog.Combo
{
    public class ComboViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ImagePath { get; set; }
        public bool IsAvailable { get; set; }
        public int ProductCount { get; set; }
        public List<ComboProductViewModel> ComboProducts { get; set; }
    }
} 