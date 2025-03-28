using GoFood.Data.Enums;
using GoFood.ViewModels.Catalog.Promotions;
using GoFood.ViewModels.Common;

namespace GoFood.ApiIntegration.Services
{
    public interface IPromotionApiClient
    {
        Task<PromotionViewModel> Create(CreatePromotionRequest request);
        Task<PromotionViewModel> Update(UpdatePromotionRequest request);
        Task<bool> UpdateStatus(Guid id, PromotionStatus status);
        Task<PromotionViewModel> GetById(Guid id);
        Task<PagedResult<PromotionViewModel>> GetAllPaging(GetPromotionPagingRequest request);
    }
}
