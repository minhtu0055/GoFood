using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using GoFood.ViewModels.Catalog.Combo;
using GoFood.ViewModels.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace GoFood.ApiIntegration.Services
{
    public class ComboApiClient : IComboApiClient
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ComboApiClient(IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            IHttpContextAccessor httpContextAccessor)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<List<ComboViewModel>> GetAll()
        {
            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new Uri(_configuration["BaseAddress"]);
            var sessions = _httpContextAccessor.HttpContext.Session.GetString("Token");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", sessions);
            var response = await client.GetAsync($"/api/combo/getall");
            var body = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
                var settings = new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    MissingMemberHandling = MissingMemberHandling.Ignore
                };
                return JsonConvert.DeserializeObject<List<ComboViewModel>>(body, settings);
            }
            throw new Exception("Không thể lấy danh sách combo");
        }

        public async Task<PagedResult<ComboViewModel>> GetAllPaging(GetComboPagingRequest request)
        {
            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new Uri(_configuration["BaseAddress"]);
            var sessions = _httpContextAccessor.HttpContext.Session.GetString("Token");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", sessions);

            var queryParams = new List<string>
            {
                $"pageIndex={request.PageIndex}",
                $"pageSize={request.PageSize}"
            };

            if (!string.IsNullOrEmpty(request.Keyword))
                queryParams.Add($"keyword={Uri.EscapeDataString(request.Keyword)}");

            if (!string.IsNullOrEmpty(request.SortBy))
                queryParams.Add($"sortBy={Uri.EscapeDataString(request.SortBy)}");

            if (request.MinPrice.HasValue)
                queryParams.Add($"minPrice={request.MinPrice.Value}");

            if (request.MaxPrice.HasValue)
                queryParams.Add($"maxPrice={request.MaxPrice.Value}");

            var url = $"/api/combo/paging?{string.Join("&", queryParams)}";
            var response = await client.GetAsync(url);
            var body = await response.Content.ReadAsStringAsync();

            var data = new PagedResult<ComboViewModel>()
            {
                Items = new List<ComboViewModel>(),
                PageIndex = request.PageIndex,
                PageSize = request.PageSize,
                TotalRecords = 0
            };
            
            if (response.IsSuccessStatusCode)
            {
                var settings = new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    MissingMemberHandling = MissingMemberHandling.Ignore
                };

                var apiResponse = JsonConvert.DeserializeObject<ApiSuccessResult<PagedResult<ComboViewModel>>>(body, settings);
                if (apiResponse?.ResultObj?.Items != null)
                {
                    data = apiResponse.ResultObj;
                }
            }

            return data;
        }

        public async Task<ComboViewModel> GetById(Guid id)
        {
            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new Uri(_configuration["BaseAddress"]);
            var sessions = _httpContextAccessor.HttpContext.Session.GetString("Token");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", sessions);
            var response = await client.GetAsync($"/api/combo/{id}");
            var body = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var settings = new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    MissingMemberHandling = MissingMemberHandling.Ignore
                };
                return JsonConvert.DeserializeObject<ComboViewModel>(body, settings);
            }
            throw new Exception("Không thể lấy thông tin combo");
        }

        public async Task<ComboViewModel> Create(ComboCreateRequest request)
        {
            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new Uri(_configuration["BaseAddress"]);
            var sessions = _httpContextAccessor.HttpContext.Session.GetString("Token");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", sessions);

            var requestContent = new MultipartFormDataContent();

            if (request.Image != null)
            {
                byte[] data;
                using (var br = new BinaryReader(request.Image.OpenReadStream()))
                {
                    data = br.ReadBytes((int)request.Image.Length);
                }
                ByteArrayContent bytes = new ByteArrayContent(data);
                requestContent.Add(bytes, "image", request.Image.FileName);
            }

            requestContent.Add(new StringContent(request.Name), "Name");
            requestContent.Add(new StringContent(request.Description ?? ""), "Description");
            requestContent.Add(new StringContent(request.Price.ToString()), "Price");
            requestContent.Add(new StringContent(request.IsAvailable.ToString()), "IsAvailable");

            // Thêm từng phần tử của ProductIds và Quantities riêng biệt
            if (request.ProductIds != null)
            {
                for (int i = 0; i < request.ProductIds.Count; i++)
                {
                    requestContent.Add(new StringContent(request.ProductIds[i].ToString()), $"ProductIds[{i}]");
                }
            }

            if (request.Quantities != null)
            {
                for (int i = 0; i < request.Quantities.Count; i++)
                {
                    requestContent.Add(new StringContent(request.Quantities[i].ToString()), $"Quantities[{i}]");
                }
            }

            var response = await client.PostAsync("/api/combo", requestContent);
            var result = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var settings = new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    MissingMemberHandling = MissingMemberHandling.Ignore
                };
                return JsonConvert.DeserializeObject<ComboViewModel>(result, settings);
            }

            throw new Exception($"Không thể tạo combo. Error: {result}");
        }

        public async Task<ComboViewModel> Update(ComboUpdateRequest request)
        {
            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new Uri(_configuration["BaseAddress"]);
            var sessions = _httpContextAccessor.HttpContext.Session.GetString("Token");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", sessions);

            var requestContent = new MultipartFormDataContent();

            if (request.Image != null)
            {
                byte[] data;
                using (var br = new BinaryReader(request.Image.OpenReadStream()))
                {
                    data = br.ReadBytes((int)request.Image.Length);
                }
                ByteArrayContent bytes = new ByteArrayContent(data);
                requestContent.Add(bytes, "image", request.Image.FileName);
            }

            requestContent.Add(new StringContent(request.Id.ToString()), "Id");
            requestContent.Add(new StringContent(request.Name), "Name");
            requestContent.Add(new StringContent(request.Description ?? ""), "Description");
            requestContent.Add(new StringContent(request.Price.ToString()), "Price");
            requestContent.Add(new StringContent(request.IsAvailable.ToString()), "IsAvailable");

            // Thêm từng phần tử của ProductIds và Quantities riêng biệt
            if (request.ProductIds != null)
            {
                for (int i = 0; i < request.ProductIds.Count; i++)
                {
                    requestContent.Add(new StringContent(request.ProductIds[i].ToString()), $"ProductIds[{i}]");
                }
            }

            if (request.Quantities != null)
            {
                for (int i = 0; i < request.Quantities.Count; i++)
                {
                    requestContent.Add(new StringContent(request.Quantities[i].ToString()), $"Quantities[{i}]");
                }
            }

            var response = await client.PutAsync("/api/combo", requestContent);
            var result = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var settings = new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    MissingMemberHandling = MissingMemberHandling.Ignore
                };
                return JsonConvert.DeserializeObject<ComboViewModel>(result, settings);
            }

            throw new Exception($"Không thể cập nhật combo. Error: {result}");
        }

        public async Task<bool> Delete(Guid id)
        {
            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new Uri(_configuration["BaseAddress"]);
            var sessions = _httpContextAccessor.HttpContext.Session.GetString("Token");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", sessions);
            var response = await client.DeleteAsync($"/api/combo/{id}");
            var result = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                return JsonConvert.DeserializeObject<bool>(result);
            }
            throw new Exception("Không thể xóa combo");
        }

        public async Task<bool> UpdateAvailability(Guid id, bool isAvailable)
        {
            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new Uri(_configuration["BaseAddress"]);
            var sessions = _httpContextAccessor.HttpContext.Session.GetString("Token");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", sessions);

            var json = JsonConvert.SerializeObject(isAvailable);
            var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PutAsync($"/api/combo/{id}/availability", httpContent);
            var result = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var settings = new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    MissingMemberHandling = MissingMemberHandling.Ignore
                };
                var apiResult = JsonConvert.DeserializeObject<ApiSuccessResult<bool>>(result, settings);
                return apiResult.ResultObj;
            }

            throw new Exception($"Không thể cập nhật trạng thái combo. Error: {result}");
        }
    }
}
