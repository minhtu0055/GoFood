using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GoFood.ViewModels.Catalog.Category;

namespace GoFood.Application.Catalog.Category
{
    public interface ICategoryService
    {
        Task<List<CategoryViewModels>> GetAll();
        Task<CategoryViewModels> GetById(Guid id);
        Task<CategoryViewModels> Create(CategoryCreateRequest request);
        Task<CategoryViewModels> Update(CategoryUpdateRequest request);
        Task<bool> Delete(Guid id);

    }
}
