using GoFood.Data.Enums;
using GoFood.ViewModels.Catalog.Promotions;
using GoFood.ViewModels.Common;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;

namespace GoFood.ApiIntegration.Services
{
    public class PromotionApiClient : IPromotionApiClient
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public PromotionApiClient(IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            IHttpContextAccessor httpContextAccessor)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<PromotionViewModel> Create(CreatePromotionRequest request)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                client.BaseAddress = new Uri(_configuration["BaseAddress"]);
                var sessions = _httpContextAccessor.HttpContext.Session.GetString("Token");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", sessions);

                var json = JsonConvert.SerializeObject(request);
                var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync("/api/promotions", httpContent);
                var result = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    var apiResult = JsonConvert.DeserializeObject<ApiSuccessResult<PromotionViewModel>>(result);
                    return apiResult.ResultObj;
                }

                throw new Exception("Không thể tạo khuyến mãi");
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi tạo khuyến mãi: {ex.Message}");
            }
        }

        public async Task<PagedResult<PromotionViewModel>> GetAllPaging(GetPromotionPagingRequest request)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                client.BaseAddress = new Uri(_configuration["BaseAddress"]);
                var sessions = _httpContextAccessor.HttpContext.Session.GetString("Token");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", sessions);

                var response = await client.GetAsync($"/api/promotions?pageIndex={request.PageIndex}" +
                    $"&pageSize={request.PageSize}" +
                    $"&keyword={request.Keyword}" +
                    $"&status={request.Status}");
                var body = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    var settings = new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Ignore,
                        MissingMemberHandling = MissingMemberHandling.Ignore
                    };
                    var apiResult = JsonConvert.DeserializeObject<ApiSuccessResult<PagedResult<PromotionViewModel>>>(body, settings);
                    var pagedResult = apiResult?.ResultObj ?? new PagedResult<PromotionViewModel>();
                    pagedResult.Items = pagedResult.Items ?? new List<PromotionViewModel>();
                    return pagedResult;
                }

                throw new Exception("Không thể lấy danh sách khuyến mãi");
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi lấy danh sách khuyến mãi: {ex.Message}");
            }
        }

        public async Task<PromotionViewModel> GetById(Guid id)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                client.BaseAddress = new Uri(_configuration["BaseAddress"]);
                var sessions = _httpContextAccessor.HttpContext.Session.GetString("Token");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", sessions);
                var response = await client.GetAsync($"/api/promotions/{id}");
                var body = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    var promotion = JsonConvert.DeserializeObject<ApiSuccessResult<PromotionViewModel>>(body);
                    return promotion.ResultObj;
                }
                
                throw new Exception("Không thể lấy thông tin khuyến mãi");
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi lấy thông tin khuyến mãi: {ex.Message}");
            }
        }

        public async Task<PromotionViewModel> Update(UpdatePromotionRequest request)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                client.BaseAddress = new Uri(_configuration["BaseAddress"]);
                var sessions = _httpContextAccessor.HttpContext.Session.GetString("Token");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", sessions);

                var json = JsonConvert.SerializeObject(request);
                var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PutAsync($"/api/promotions/{request.Id}", httpContent);
                var result = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    var apiResult = JsonConvert.DeserializeObject<ApiSuccessResult<PromotionViewModel>>(result);
                    return apiResult.ResultObj;
                }

                throw new Exception("Không thể cập nhật khuyến mãi");
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi cập nhật khuyến mãi: {ex.Message}");
            }
        }

        public async Task<bool> UpdateStatus(Guid id, PromotionStatus status)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                client.BaseAddress = new Uri(_configuration["BaseAddress"]);
                var sessions = _httpContextAccessor.HttpContext.Session.GetString("Token");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", sessions);

                var json = JsonConvert.SerializeObject(status);
                var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PatchAsync($"/api/promotions/{id}/status", httpContent);
                var result = await response.Content.ReadAsStringAsync();
                
                if (response.IsSuccessStatusCode)
                {
                    var apiResult = JsonConvert.DeserializeObject<ApiSuccessResult<bool>>(result);
                    return apiResult.ResultObj;
                }

                throw new Exception("Không thể cập nhật trạng thái khuyến mãi");
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi cập nhật trạng thái khuyến mãi: {ex.Message}");
            }
        }
    }
}
