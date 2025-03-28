using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GoFood.Data.Entities;
using GoFood.Data.Enums;
using GoFood.ViewModels.Catalog.Products;
using GoFood.ViewModels.Common;
using Microsoft.AspNetCore.Http;

namespace GoFood.Application.Catalog.Products
{
    public interface IProductService
    {
        Task<ProductViewModels> Create(ProductCreateRequest request);
        Task<ProductViewModels> Update(ProductUpdateRequest request);
        Task<bool> UpdateStatus(Guid id, ProductStatus status);
        Task<ProductViewModels> GetById(Guid id);
        Task<PagedResult<ProductViewModels>> GetAllPaging(GetProductPagingRequest request);
        Task<List<ProductViewModels>> GetAll();
        Task<bool> UpdatePrice(Guid id, decimal newPrice);
        Task<bool> UpdateStock(Guid id, int addedQuantity);
        Task<int> AddImage(Guid productId, IFormFile image);
        Task<int> RemoveImage(Guid imageId);
        Task<List<string>> GetListImages(Guid productId);
    }
}
