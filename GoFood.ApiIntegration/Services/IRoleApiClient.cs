using GoFood.ViewModels.Common;
using GoFood.ViewModels.System.Roles;

namespace GoFood.ApiIntegration.Services
{
    public interface IRoleApiClient
    {
        Task<ApiResult<List<RoleViewModel>>> GetAll();
    }
}
