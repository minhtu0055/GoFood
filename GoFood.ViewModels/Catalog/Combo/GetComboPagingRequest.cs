using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GoFood.ViewModels.Common;
using GoFood.ViewModels.System.Users;

namespace GoFood.ViewModels.Catalog.Combo
{
    public class GetComboPagingRequest : PagingRequestBase
    {
        public string? Keyword { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public string? SortBy { get; set; } // Có thể là "price_asc", "price_desc", "name_asc", "name_desc"
    }
} 