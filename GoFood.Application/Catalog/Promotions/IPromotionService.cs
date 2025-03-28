using System;
using System.Threading.Tasks;
using GoFood.Data.Enums;
using GoFood.ViewModels.Catalog.Products;
using GoFood.ViewModels.Catalog.Promotions;
using GoFood.ViewModels.Common;

namespace GoFood.Application.Catalog.Promotions
{
    public interface IPromotionService
    {
        Task<PagedResult<PromotionViewModel>> GetAllPaging(GetPromotionPagingRequest request);
        Task<PromotionViewModel> GetById(Guid id);
        Task<PromotionViewModel> Create(CreatePromotionRequest request);
        Task<PromotionViewModel> Update(UpdatePromotionRequest request);
        Task<bool> UpdateStatus(Guid id, PromotionStatus status);
    }
} 