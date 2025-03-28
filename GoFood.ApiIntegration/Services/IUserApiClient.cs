using GoFood.ViewModels.Common;
using GoFood.ViewModels.System.Users;


namespace GoFood.ApiIntegration.Services
{
    public interface IUserApiClient
    {
        Task<ApiResult<string>> Authenticate(LoginRequest request);
        Task<ApiResult<bool>> Register(RegisterRequest request);
        Task<ApiResult<bool>> UpdateUser(Guid id, UserUpdateRequest request);
        Task<ApiResult<PagedResult<UserViewModels>>> GetUsersPagings(GetUserPagingRequest request);
        Task<ApiResult<UserViewModels>> GetById(Guid id);
        Task<ApiResult<bool>> RoleAssign(Guid id, RoleAssignRequest request);
        Task<ApiResult<bool>> Delete(Guid id, bool isActive);
        Task<ApiResult<bool>> ForgotPassword(ForgotPasswordRequest request);
        Task<ApiResult<bool>> ChangePassword(Guid id, ChangePasswordRequest request);
    }
}
