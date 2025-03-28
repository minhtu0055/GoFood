using GoFood.ViewModels.Catalog.Combo;
using GoFood.ViewModels.Common;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GoFood.ApiIntegration.Services
{
    public interface IComboApiClient
    {
        Task<List<ComboViewModel>> GetAll();
        Task<PagedResult<ComboViewModel>> GetAllPaging(GetComboPagingRequest request);
        Task<ComboViewModel> GetById(Guid id);
        Task<ComboViewModel> Create(ComboCreateRequest request);
        Task<ComboViewModel> Update(ComboUpdateRequest request);
        Task<bool> Delete(Guid id);
        Task<bool> UpdateAvailability(Guid id, bool isAvailable);
    } 
}
