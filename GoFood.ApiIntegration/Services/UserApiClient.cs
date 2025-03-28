using System;
using System.Net.Http.Headers;
using System.Text;
using GoFood.ViewModels.Common;
using GoFood.ViewModels.System.Users;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace GoFood.ApiIntegration.Services
{
    public class UserApiClient : IUserApiClient
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserApiClient(IConfiguration configuration, IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor)
        {
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<ApiResult<string>> Authenticate(LoginRequest request)
        {
            var json = JsonConvert.SerializeObject(request);
            var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new Uri(_configuration["BaseAddress"]);
            var response = await client.PostAsync("/api/user/authenticate", httpContent);
            if (response.IsSuccessStatusCode)
            {
                return JsonConvert.DeserializeObject<ApiSuccessResult<string>>(await response.Content.ReadAsStringAsync());
            }

            return JsonConvert.DeserializeObject<ApiErrorResult<string>>(await response.Content.ReadAsStringAsync());
        }

        public async Task<ApiResult<bool>> Delete(Guid id, bool isActive)
        {
            try
            {
                var sessions = _httpContextAccessor.HttpContext.Session.GetString("Token");
                var client = _httpClientFactory.CreateClient();

                client.BaseAddress = new Uri(_configuration["BaseAddress"]);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", sessions);

                var content = new StringContent("", Encoding.UTF8, "application/json");
                var response = await client.PostAsync($"/api/user/{(isActive ? "active" : "deactivate")}/{id}", content);
                var body = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    return JsonConvert.DeserializeObject<ApiSuccessResult<bool>>(body) 
                           ?? new ApiSuccessResult<bool>();
                }
                return JsonConvert.DeserializeObject<ApiErrorResult<bool>>(body) 
                       ?? new ApiErrorResult<bool>("Lỗi không xác định từ server");
            }
            catch (Exception ex)
            {
                return new ApiErrorResult<bool>($"Lỗi khi gọi API: {ex.Message}");
            }
        }

        public async Task<ApiResult<UserViewModels>> GetById(Guid id)
        {
            var sessions = _httpContextAccessor.HttpContext.Session.GetString("Token"); // lấy chuỗi session từ token
            var client = _httpClientFactory.CreateClient(sessions);
            client.BaseAddress = new Uri(_configuration["BaseAddress"]);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", sessions); // thiết lập header bearer thường được dùng với JWT
            //Gửi yều cầu get đến api 
            var response = await client.GetAsync($"/api/user/{id}"); // await đảm bảo gọi API bất đồng bộ, không làm chặn chương trình
            var body = await response.Content.ReadAsStringAsync(); // đọc nội dung phần hồi từ API dưới dạng Json
            //Kiểm tra phản hồi
            if (response.IsSuccessStatusCode)
            {
                return JsonConvert.DeserializeObject<ApiSuccessResult<UserViewModels>>(body);
            }
            return JsonConvert.DeserializeObject<ApiErrorResult<UserViewModels>>(body);
        }

        public async Task<ApiResult<PagedResult<UserViewModels>>> GetUsersPagings(GetUserPagingRequest request)
        {
            var client = _httpClientFactory.CreateClient();
            var sessions = _httpContextAccessor.HttpContext.Session.GetString("Token");

            client.BaseAddress = new Uri(_configuration["BaseAddress"]);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", sessions);
            var response = await client.GetAsync($"/api/user/paging?pageIndex=" +
                $"{request.PageIndex}&pageSize={request.PageSize}&keyword={request.Keyword}");
            var body = await response.Content.ReadAsStringAsync();
            var users = JsonConvert.DeserializeObject<ApiSuccessResult<PagedResult<UserViewModels>>>(body);
            return users;
        }

        public async Task<ApiResult<bool>> Register(RegisterRequest request)
        {
            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new Uri(_configuration["BaseAddress"]);

            var json = JsonConvert.SerializeObject(request);
            var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync($"/api/user/register", httpContent);
            var result = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
                return JsonConvert.DeserializeObject<ApiSuccessResult<bool>>(result);

            return JsonConvert.DeserializeObject<ApiErrorResult<bool>>(result);
        }

        public async Task<ApiResult<bool>> RoleAssign(Guid id, RoleAssignRequest request)
        {
            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new Uri(_configuration["BaseAddress"]);
            var sessions = _httpContextAccessor.HttpContext.Session.GetString("Token");
            
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", sessions);
            
            var json = JsonConvert.SerializeObject(request);
            var httpContent = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await client.PutAsync($"/api/user/{id}/roles", httpContent);
            var result = await response.Content.ReadAsStringAsync();
            
            if (response.IsSuccessStatusCode)
                return JsonConvert.DeserializeObject<ApiSuccessResult<bool>>(result);
            
            return JsonConvert.DeserializeObject<ApiErrorResult<bool>>(result);
        }

        public async Task<ApiResult<bool>> UpdateUser(Guid id, UserUpdateRequest request)
        {
            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new Uri(_configuration["BaseAddress"]);
            var sessions = _httpContextAccessor.HttpContext.Session.GetString("Token");

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", sessions);

            var json = JsonConvert.SerializeObject(request);
            var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PutAsync($"/api/user/{id}", httpContent);
            var result = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
                return JsonConvert.DeserializeObject<ApiSuccessResult<bool>>(result);
            }
            return JsonConvert.DeserializeObject<ApiErrorResult<bool>>(result);
        }
        public async Task<ApiResult<bool>> ForgotPassword(ForgotPasswordRequest request)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                client.BaseAddress = new Uri(_configuration["BaseAddress"]);

                var json = JsonConvert.SerializeObject(request);
                var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync($"/api/user/forgot-password", httpContent);
                var body = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonConvert.DeserializeObject<ApiSuccessResult<bool>>(body);
                    if (result != null)
                        return result;
                        
                    return new ApiSuccessResult<bool> { 
                        IsSuccessed = true,
                        Message = "Mật khẩu mới đã được gửi đến email của bạn. Vui lòng kiểm tra email và đăng nhập. Sau khi đăng nhập thành công, bạn nên đổi lại mật khẩu mới để đảm bảo tính bảo mật.",
                        ResultObj = true
                    };
                }

                return new ApiErrorResult<bool>("Có lỗi xảy ra khi xử lý yêu cầu. Vui lòng thử lại sau.");
            }
            catch (Exception ex)
            {
                return new ApiErrorResult<bool>($"Lỗi kết nối: {ex.Message}");
            }
        }

        public async Task<ApiResult<bool>> ChangePassword(Guid id, ChangePasswordRequest request)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                client.BaseAddress = new Uri(_configuration["BaseAddress"]);
                var sessions = _httpContextAccessor.HttpContext.Session.GetString("Token");

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", sessions);

                var json = JsonConvert.SerializeObject(request);
                var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync($"/api/user/change-password/{id}", httpContent);
                var body = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonConvert.DeserializeObject<ApiSuccessResult<bool>>(body);
                    return result ?? new ApiSuccessResult<bool>
                    {
                        IsSuccessed = true,
                        Message = "Đổi mật khẩu thành công",
                        ResultObj = true
                    };
                }

                var error = JsonConvert.DeserializeObject<ApiErrorResult<bool>>(body);
                return error ?? new ApiErrorResult<bool>("Có lỗi xảy ra khi đổi mật khẩu");
            }
            catch (Exception ex)
            {
                return new ApiErrorResult<bool>($"Lỗi: {ex.Message}");
            }
        }

    }
}
