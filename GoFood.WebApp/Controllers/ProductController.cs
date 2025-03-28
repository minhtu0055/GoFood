using GoFood.ApiIntegration.Services;
using GoFood.Data.Enums;
using GoFood.ViewModels.Catalog.Products;
using Microsoft.AspNetCore.Mvc;

namespace GoFood.WebApp.Controllers
{
    public class ProductController : Controller
    {
        private readonly IProductApiClient _productApiClient;
        private readonly ICategoryApiClient _categoryApiClient;

        public ProductController(IProductApiClient productApiClient, 
            ICategoryApiClient categoryApiClient)
        {
            _productApiClient = productApiClient;
            _categoryApiClient = categoryApiClient;
        }

        public async Task<IActionResult> Index(string keyword, Guid? categoryId, int pageIndex = 1)
        {
            var categories = await _categoryApiClient.GetAll();
            
            var request = new GetProductPagingRequest()
            {
                Keyword = keyword,
                PageIndex = pageIndex,
                PageSize = 12,
                CategoryId = categoryId,
                Status = ProductStatus.Active,
                FilterByStatus = true
            };

            var products = await _productApiClient.GetAllPaging(request);

            var viewModel = new ProductCategoryViewModel
            {
                Categories = categories,
                Products = products,
                CategoryId = categoryId
            };

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> GetProductsByCategory(Guid categoryId)
        {
            var request = new GetProductPagingRequest()
            {
                CategoryId = categoryId,
                PageSize = 12,
                PageIndex = 1,
                Status = ProductStatus.Active,
                FilterByStatus = true
            };
            
            var products = await _productApiClient.GetAllPaging(request);
            return PartialView("_ProductList", products);
        }
    }
}
