using GoFood.ViewModels.Catalog.Category;
using GoFood.ViewModels.Catalog.Products;
using GoFood.ViewModels.Common;

namespace GoFood.ViewModels.Catalog.Products
{
    public class ProductCategoryViewModel
    {
        public List<CategoryViewModels> Categories { get; set; }
        public PagedResult<ProductViewModels> Products { get; set; }
        public Guid? CategoryId { get; set; }
    }
}