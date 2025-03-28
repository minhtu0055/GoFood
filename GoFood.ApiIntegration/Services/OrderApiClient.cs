using GoFood.ViewModels.Catalog.Orders;
using GoFood.ViewModels.Common;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GoFood.ApiIntegration.Services
{
    public class OrderApiClient : IOrderApiClient

    {
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public OrderApiClient(IConfiguration configuration, IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor)
        {
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<PagedResult<OrderViewModel>> GetAllPaging(GetOrderPagingRequest request)
        {
            var client = _httpClientFactory.CreateClient();
            var session = _httpContextAccessor.HttpContext.Session.GetString("Token");
            client.BaseAddress = new Uri(_configuration["BaseAddress"]);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", session);

            var url = $"/api/orders/paging?pageIndex={request.PageIndex}&pageSize={request.PageSize}";
            
            if (!string.IsNullOrEmpty(request.Keyword))
                url += $"&keyword={request.Keyword}";
            if (request.Status.HasValue)
                url += $"&status={request.Status}";
            if (request.FromDate.HasValue)
                url += $"&fromDate={request.FromDate.Value:yyyy-MM-dd}";
            if (request.ToDate.HasValue)
                url += $"&toDate={request.ToDate.Value:yyyy-MM-dd}";
            if (request.UserId.HasValue)
                url += $"&userId={request.UserId}";

            var response = await client.GetAsync(url);
            var body = await response.Content.ReadAsStringAsync();
        
            
            if (response.IsSuccessStatusCode)
            {
                try
                {
                    var result = JsonConvert.DeserializeObject<ApiSuccessResult<PagedResult<OrderViewModel>>>(body);
                    System.Diagnostics.Debug.WriteLine($"Deserialized Result: {result != null}");
                    System.Diagnostics.Debug.WriteLine($"ResultObj: {result?.ResultObj != null}");
                    
                    if (result?.ResultObj != null)
                    {
                        return result.ResultObj;
                    }
                }
                catch (JsonException ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Deserialization Error: {ex.Message}");
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"API Error: {response.StatusCode} - {body}");
            }
            
            return new PagedResult<OrderViewModel>
            {
                Items = new List<OrderViewModel>(),
                PageIndex = request.PageIndex,
                PageSize = request.PageSize,
                TotalRecords = 0
            };
        }

        public async Task<OrderViewModel> GetById(Guid id)
        {
            var client = _httpClientFactory.CreateClient();
            var session = _httpContextAccessor.HttpContext.Session.GetString("Token");
            client.BaseAddress = new Uri(_configuration["BaseAddress"]);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", session);

            var response = await client.GetAsync($"/api/orders/{id}");
            var body = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
                var result = JsonConvert.DeserializeObject<ApiSuccessResult<OrderViewModel>>(body);
                return result.ResultObj;
            }
            throw new Exception("Cannot get order");
        }

        public async Task<List<OrderViewModel>> GetOrdersByUserId(Guid userId)
        {
            var client = _httpClientFactory.CreateClient();
            var session = _httpContextAccessor.HttpContext.Session.GetString("Token");
            client.BaseAddress = new Uri(_configuration["BaseAddress"]);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", session);

            var response = await client.GetAsync($"/api/orders/user/{userId}");
            var body = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
                var result = JsonConvert.DeserializeObject<ApiSuccessResult<List<OrderViewModel>>>(body);
                return result.ResultObj;
            }
            throw new Exception("Cannot get orders by user id");
        }

        public async Task<bool> UpdateStatus(Guid id, OrderStatusUpdateRequest request)
        {
            var client = _httpClientFactory.CreateClient();
            var session = _httpContextAccessor.HttpContext.Session.GetString("Token");
            client.BaseAddress = new Uri(_configuration["BaseAddress"]);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", session);

            var json = JsonConvert.SerializeObject(request);
            var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PatchAsync($"/api/orders/{id}/status", httpContent);
            var body = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
                var result = JsonConvert.DeserializeObject<ApiSuccessResult<bool>>(body);
                return result.ResultObj;
            }
            throw new Exception("Cannot update order status");
        }

        public async Task<bool> CancelOrder(Guid id)
        {
            var client = _httpClientFactory.CreateClient();
            var session = _httpContextAccessor.HttpContext.Session.GetString("Token");
            client.BaseAddress = new Uri(_configuration["BaseAddress"]);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", session);

            var response = await client.PostAsync($"/api/orders/{id}/cancel", null);
            var body = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
                var result = JsonConvert.DeserializeObject<ApiSuccessResult<bool>>(body);
                return result.ResultObj;
            }
            throw new Exception("Cannot cancel order");
        }

        public async Task<OrderViewModel> ExportInvoice(Guid id)
        {
            var client = _httpClientFactory.CreateClient();
            var session = _httpContextAccessor.HttpContext.Session.GetString("Token");
            client.BaseAddress = new Uri(_configuration["BaseAddress"]);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", session);

            var response = await client.GetAsync($"/api/orders/{id}/invoice");
            var body = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
                var result = JsonConvert.DeserializeObject<ApiSuccessResult<OrderViewModel>>(body);
                return result.ResultObj;
            }
            throw new Exception("Cannot export invoice");
        }
    }
}