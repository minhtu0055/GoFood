using GoFood.Data.Enums;
using GoFood.ViewModels.Catalog.Products;
using GoFood.ViewModels.Common;
using Microsoft.AspNetCore.Http;

namespace GoFood.ApiIntegration.Services
{
    public interface IProductApiClient
    {
        Task<List<ProductViewModels>> GetAll();
        Task<PagedResult<ProductViewModels>> GetAllPaging(GetProductPagingRequest request);
        Task<ProductViewModels> GetById(Guid id);
        Task<ProductViewModels> Create(ProductCreateRequest request);
        Task<ProductViewModels> Update(ProductUpdateRequest request);
        Task<bool> UpdatePrice(Guid id, decimal newPrice);
        Task<bool> UpdateStatus(Guid id, ProductStatus status);
        Task<bool> UpdateStock(Guid id, int addedQuantity);
        Task<int> AddImage(Guid productId, IFormFile image);
        Task<int> RemoveImage(Guid imageId);
        Task<List<string>> GetListImages(Guid productId);
    }
}
