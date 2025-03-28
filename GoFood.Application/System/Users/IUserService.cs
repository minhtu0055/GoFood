using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GoFood.ViewModels.Common;
using GoFood.ViewModels.System.Users;

namespace GoFood.Application.System.Users
{
    public interface IUserService
    {
        Task<ApiResult<string>> Authenticate(LoginRequest request);
        Task<ApiResult<bool>> Register(RegisterRequest request);
        Task<ApiResult<bool>> Update(Guid id, UserUpdateRequest request);
        Task<ApiResult<PagedResult<UserViewModels>>> GetUsersPaging(GetUserPagingRequest request);
        Task<ApiResult<UserViewModels>> GetById(Guid id);
        Task<ApiResult<bool>> RoleAssign(Guid id, RoleAssignRequest request);
        Task<ApiResult<bool>> Delete(Guid id, bool isActive);
        Task<ApiResult<bool>> ForgotPassword(string email);
        Task<ApiResult<bool>> ChangePassword(Guid id, ChangePasswordRequest request);
    }
}
