using GoFood.Data.Enums;
using GoFood.ViewModels.Catalog.Vouchers;
using GoFood.ViewModels.Common;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;

namespace GoFood.ApiIntegration.Services
{
    public class VoucherApiClient : IVoucherApiClient
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public VoucherApiClient(IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            IHttpContextAccessor httpContextAccessor)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<VoucherViewModel> Create(CreateVoucherRequest request)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                client.BaseAddress = new Uri(_configuration["BaseAddress"]);
                var sessions = _httpContextAccessor.HttpContext.Session.GetString("Token");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", sessions);

                var json = JsonConvert.SerializeObject(request);
                var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync("/api/vouchers", httpContent);
                var result = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    var apiResult = JsonConvert.DeserializeObject<ApiSuccessResult<VoucherViewModel>>(result);
                    return apiResult.ResultObj;
                }

                throw new Exception("Không thể tạo voucher");
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi tạo voucher: {ex.Message}");
            }
        }

        public async Task<PagedResult<VoucherViewModel>> GetAllPaging(GetVoucherPagingRequest request)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                client.BaseAddress = new Uri(_configuration["BaseAddress"]);
                var sessions = _httpContextAccessor.HttpContext.Session.GetString("Token");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", sessions);

                var response = await client.GetAsync($"/api/vouchers?pageIndex={request.PageIndex}" +
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
                    var apiResult = JsonConvert.DeserializeObject<ApiSuccessResult<PagedResult<VoucherViewModel>>>(body, settings);
                    var pagedResult = apiResult?.ResultObj ?? new PagedResult<VoucherViewModel>();
                    pagedResult.Items = pagedResult.Items ?? new List<VoucherViewModel>();
                    return pagedResult;
                }

                throw new Exception("Không thể lấy danh sách voucher");
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi lấy danh sách voucher: {ex.Message}");
            }
        }

        public async Task<VoucherViewModel> GetByCode(string code)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                client.BaseAddress = new Uri(_configuration["BaseAddress"]);
                var sessions = _httpContextAccessor.HttpContext.Session.GetString("Token");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", sessions);
                var response = await client.GetAsync($"/api/vouchers/code/{code}");
                var body = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    var voucher = JsonConvert.DeserializeObject<ApiSuccessResult<VoucherViewModel>>(body);
                    return voucher.ResultObj;
                }
                
                throw new Exception("Không thể lấy thông tin voucher");
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi lấy thông tin voucher: {ex.Message}");
            }
        }

        public async Task<VoucherViewModel> GetById(Guid id)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                client.BaseAddress = new Uri(_configuration["BaseAddress"]);
                var sessions = _httpContextAccessor.HttpContext.Session.GetString("Token");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", sessions);
                var response = await client.GetAsync($"/api/vouchers/{id}");
                var body = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    var voucher = JsonConvert.DeserializeObject<ApiSuccessResult<VoucherViewModel>>(body);
                    return voucher.ResultObj;
                }
                
                throw new Exception("Không thể lấy thông tin voucher");
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi lấy thông tin voucher: {ex.Message}");
            }
        }

        public async Task<VoucherViewModel> Update(UpdateVoucherRequest request)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                client.BaseAddress = new Uri(_configuration["BaseAddress"]);
                var sessions = _httpContextAccessor.HttpContext.Session.GetString("Token");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", sessions);

                var json = JsonConvert.SerializeObject(request);
                var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PutAsync($"/api/vouchers/{request.Id}", httpContent);
                var result = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    var apiResult = JsonConvert.DeserializeObject<ApiSuccessResult<VoucherViewModel>>(result);
                    return apiResult.ResultObj;
                }

                throw new Exception("Không thể cập nhật voucher");
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi cập nhật voucher: {ex.Message}");
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

                var response = await client.PatchAsync($"/api/vouchers/{id}/status", httpContent);
                var result = await response.Content.ReadAsStringAsync();
                
                if (response.IsSuccessStatusCode)
                {
                    var apiResult = JsonConvert.DeserializeObject<ApiSuccessResult<bool>>(result);
                    return apiResult.ResultObj;
                }

                throw new Exception("Không thể cập nhật trạng thái voucher");
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi cập nhật trạng thái voucher: {ex.Message}");
            }
        }

        public async Task<bool> UseVoucher(string code)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                client.BaseAddress = new Uri(_configuration["BaseAddress"]);
                var sessions = _httpContextAccessor.HttpContext.Session.GetString("Token");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", sessions);

                var response = await client.PostAsync($"/api/vouchers/code/{code}/use", null);
                var result = await response.Content.ReadAsStringAsync();
                
                if (response.IsSuccessStatusCode)
                {
                    var apiResult = JsonConvert.DeserializeObject<ApiSuccessResult<bool>>(result);
                    return apiResult.ResultObj;
                }

                throw new Exception("Không thể sử dụng voucher");
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi sử dụng voucher: {ex.Message}");
            }
        }
    }
}
