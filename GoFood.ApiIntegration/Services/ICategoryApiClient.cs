using GoFood.ViewModels.Catalog.Category;

namespace GoFood.ApiIntegration.Services
{
    public interface ICategoryApiClient
    {
        Task<List<CategoryViewModels>> GetAll();
        Task<CategoryViewModels> GetById(Guid id);
        Task<CategoryViewModels> Create(CategoryCreateRequest request);
        Task<CategoryViewModels> Update(CategoryUpdateRequest request);
    }
} 