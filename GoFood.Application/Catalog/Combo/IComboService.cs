using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GoFood.ViewModels.Catalog.Combo;
using GoFood.ViewModels.Common;

namespace GoFood.Application.Catalog.Combo
{
    public interface IComboService
    {
        Task<ComboViewModel> GetById(Guid id);
        Task<PagedResult<ComboViewModel>> GetAllPaging(GetComboPagingRequest request);
        Task<List<ComboViewModel>> GetAll();
        Task<ComboViewModel> Create(ComboCreateRequest request);
        Task<ComboViewModel> Update(ComboUpdateRequest request);
        Task<bool> Delete(Guid id);
        Task<bool> UpdateAvailability(Guid id, bool isAvailable);
    }
} 