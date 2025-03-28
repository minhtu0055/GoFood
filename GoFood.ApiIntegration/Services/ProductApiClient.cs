using GoFood.Data.Enums;
using GoFood.ViewModels.Catalog.Products;
using GoFood.ViewModels.Common;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;

namespace GoFood.ApiIntegration.Services
{
    public class ProductApiClient : IProductApiClient
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ProductApiClient(IHttpClientFactory httpClientFactory, 
            IConfiguration configuration,
            IHttpContextAccessor httpContextAccessor)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<List<ProductViewModels>> GetAll()
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                client.BaseAddress = new Uri(_configuration["BaseAddress"]);
                var sessions = _httpContextAccessor.HttpContext.Session.GetString("Token");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", sessions);
                var response = await client.GetAsync($"/api/products/getall");
                var body = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    var settings = new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Ignore,
                        MissingMemberHandling = MissingMemberHandling.Ignore
                    };
                    var apiResult = JsonConvert.DeserializeObject<ApiSuccessResult<List<ProductViewModels>>>(body, settings);
                    return apiResult?.ResultObj ?? new List<ProductViewModels>();
                }

                throw new Exception("Không thể lấy danh sách sản phẩm");
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi lấy danh sách sản phẩm: {ex.Message}");
            }
        }

        public async Task<ProductViewModels> Create(ProductCreateRequest request)
        {
            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new Uri(_configuration["BaseAddress"]);

            var sessions = _httpContextAccessor.HttpContext.Session.GetString("Token");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", sessions);

            var requestContent = new MultipartFormDataContent();

            if (request.Images?.Count > 0)
            {
                foreach (var image in request.Images)
                {
                    byte[] data;
                    using (var br = new BinaryReader(image.OpenReadStream()))
                    {
                        data = br.ReadBytes((int)image.Length);
                    }
                    ByteArrayContent bytes = new ByteArrayContent(data);
                    requestContent.Add(bytes, "images", image.FileName);
                }
            }

            requestContent.Add(new StringContent(request.Name.ToString()), "name");
            requestContent.Add(new StringContent(request.Description.ToString()), "description");
            requestContent.Add(new StringContent(request.Price.ToString()), "price");
            requestContent.Add(new StringContent(request.Quantity.ToString()), "quantity");
            requestContent.Add(new StringContent(request.CategoryId.ToString()), "categoryId");

            var response = await client.PostAsync($"/api/products", requestContent);
            var result = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
                return JsonConvert.DeserializeObject<ProductViewModels>(result);

            throw new Exception("Không thể tạo sản phẩm");
        }

        public async Task<PagedResult<ProductViewModels>> GetAllPaging(GetProductPagingRequest request)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                client.BaseAddress = new Uri(_configuration["BaseAddress"]);
                var sessions = _httpContextAccessor.HttpContext.Session.GetString("Token");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", sessions);
                var response = await client.GetAsync($"/api/products?pageIndex={request.PageIndex}" +
                    $"&pageSize={request.PageSize}" +
                    $"&keyword={request.Keyword}" +
                    $"&categoryId={request.CategoryId}" +
                    $"&minPrice={request.MinPrice}" +
                    $"&maxPrice={request.MaxPrice}" +
                    $"&filterByStatus={request.FilterByStatus}" +
                    $"&status={request.Status}");
                var body = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    var settings = new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Ignore,
                        MissingMemberHandling = MissingMemberHandling.Ignore
                    };
                    var apiResult = JsonConvert.DeserializeObject<ApiSuccessResult<PagedResult<ProductViewModels>>>(body, settings);
                    var pagedResult = apiResult?.ResultObj ?? new PagedResult<ProductViewModels>();
                    pagedResult.Items = pagedResult.Items ?? new List<ProductViewModels>();
                    return pagedResult;
                }

                throw new Exception("Không thể lấy danh sách sản phẩm phân trang");
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi lấy danh sách sản phẩm phân trang: {ex.Message}");
            }
        }

        public async Task<ProductViewModels> GetById(Guid id)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                client.BaseAddress = new Uri(_configuration["BaseAddress"]);
                var sessions = _httpContextAccessor.HttpContext.Session.GetString("Token");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", sessions);
                var response = await client.GetAsync($"/api/products/{id}");
                var body = await response.Content.ReadAsStringAsync();
                
                Console.WriteLine($"API Response - Status: {response.StatusCode}, Body: {body}");
                
                if (response.IsSuccessStatusCode)
                {
                    var settings = new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Ignore,
                        MissingMemberHandling = MissingMemberHandling.Ignore,
                        Error = (sender, args) => {
                            args.ErrorContext.Handled = true;
                            Console.WriteLine($"Deserialize Error: {args.ErrorContext.Error.Message}");
                        }
                    };
                    
                    try {
                        var apiResult = JsonConvert.DeserializeObject<ApiSuccessResult<ProductViewModels>>(body, settings);
                        if (apiResult == null || apiResult.ResultObj == null)
                        {
                            throw new Exception($"API trả về dữ liệu không hợp lệ: {body}");
                        }
                        return apiResult.ResultObj;
                    }
                    catch (JsonException jsonEx) {
                        throw new Exception($"Lỗi phân tích JSON: {jsonEx.Message}, dữ liệu: {body}");
                    }
                }

                throw new Exception($"API trả về lỗi - Status: {response.StatusCode}, Body: {body}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in GetById: {ex.Message}");
                throw new Exception($"Lỗi khi lấy thông tin sản phẩm: {ex.Message}");
            }
        }

        public async Task<ProductViewModels> Update(ProductUpdateRequest request)
        {
            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new Uri(_configuration["BaseAddress"]);
            var sessions = _httpContextAccessor.HttpContext.Session.GetString("Token");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", sessions);

            var json = JsonConvert.SerializeObject(request);
            var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PutAsync($"/api/products/{request.Id}", httpContent);
            var result = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
                return JsonConvert.DeserializeObject<ProductViewModels>(result);

            throw new Exception("Không thể cập nhật sản phẩm");
        }

        public async Task<bool> UpdatePrice(Guid id, decimal newPrice)
        {
            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new Uri(_configuration["BaseAddress"]);
            var sessions = _httpContextAccessor.HttpContext.Session.GetString("Token");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", sessions);

            var json = JsonConvert.SerializeObject(newPrice);
            var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PatchAsync($"/api/products/{id}/price", httpContent);
            if (response.IsSuccessStatusCode)
                return true;
            
            throw new Exception("Không thể cập nhật giá sản phẩm");
        }

        public async Task<bool> UpdateStatus(Guid id, ProductStatus status)
        {
            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new Uri(_configuration["BaseAddress"]);
            var sessions = _httpContextAccessor.HttpContext.Session.GetString("Token");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", sessions);

            var json = JsonConvert.SerializeObject(status);
            var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PatchAsync($"/api/products/{id}/status", httpContent);
            if (response.IsSuccessStatusCode)
                return true;
            
            throw new Exception("Không thể cập nhật trạng thái sản phẩm");
        }

        public async Task<bool> UpdateStock(Guid id, int addedQuantity)
        {
            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new Uri(_configuration["BaseAddress"]);
            var sessions = _httpContextAccessor.HttpContext.Session.GetString("Token");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", sessions);

            var json = JsonConvert.SerializeObject(addedQuantity);
            var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PatchAsync($"/api/products/{id}/stock", httpContent);
            if (response.IsSuccessStatusCode)
                return true;
            
            throw new Exception("Không thể cập nhật số lượng sản phẩm");
        }

        public async Task<int> AddImage(Guid productId, IFormFile image)
        {
            try
            {
                // Tạo HTTP client
                var client = _httpClientFactory.CreateClient();
                client.BaseAddress = new Uri(_configuration["BaseAddress"]);
                var sessions = _httpContextAccessor.HttpContext.Session.GetString("Token");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", sessions);

                // Chuẩn bị form data
                var requestContent = new MultipartFormDataContent();
                
                if (image != null)
                {
                    byte[] data;
                    using (var br = new BinaryReader(image.OpenReadStream()))
                    {
                        data = br.ReadBytes((int)image.Length);
                    }
                    
                    ByteArrayContent bytes = new ByteArrayContent(data);
                    bytes.Headers.ContentType = new MediaTypeHeaderValue(image.ContentType);
                    requestContent.Add(bytes, "file", image.FileName);
                }

                // Gửi request
                var response = await client.PostAsync($"/api/products/{productId}/images", requestContent);
                var result = await response.Content.ReadAsStringAsync();
                
                if (response.IsSuccessStatusCode)
                {
                    return 1; // Mặc định trả về 1 nếu thành công
                }
                
                throw new Exception($"Lỗi khi thêm ảnh: {result}");
            }
            catch (Exception ex)
            {
                throw new Exception($"Không thể thêm hình ảnh: {ex.Message}");
            }
        }

        public async Task<int> RemoveImage(Guid imageId)
        {
            try 
            {
                // Tạo HTTP client
                var client = _httpClientFactory.CreateClient();
                client.BaseAddress = new Uri(_configuration["BaseAddress"]);
                var sessions = _httpContextAccessor.HttpContext.Session.GetString("Token");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", sessions);
                
                // Gửi request
                var response = await client.DeleteAsync($"/api/products/images/{imageId}");
                var result = await response.Content.ReadAsStringAsync();
                
                if (response.IsSuccessStatusCode)
                {
                    return 1; // Mặc định trả về 1 nếu thành công
                }
                
                throw new Exception($"Lỗi khi xóa ảnh: {result}");
            }
            catch (Exception ex)
            {
                throw new Exception($"Không thể xóa hình ảnh: {ex.Message}");
            }
        }

        public async Task<List<string>> GetListImages(Guid productId)
        {
            try 
            {
                var client = _httpClientFactory.CreateClient();
                client.BaseAddress = new Uri(_configuration["BaseAddress"]);
                var sessions = _httpContextAccessor.HttpContext.Session.GetString("Token");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", sessions);

                var response = await client.GetAsync($"/api/products/{productId}/images");
                var body = await response.Content.ReadAsStringAsync();
                
                Console.WriteLine($"GetListImages Response - Status: {response.StatusCode}, Body: {body}");
                
                if (response.IsSuccessStatusCode)
                {
                    var settings = new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Ignore,
                        MissingMemberHandling = MissingMemberHandling.Ignore
                    };
                    
                    try {
                        var apiResult = JsonConvert.DeserializeObject<ApiSuccessResult<List<string>>>(body, settings);
                        if (apiResult != null && apiResult.ResultObj != null)
                        {
                            return apiResult.ResultObj;
                        }
                        
                        // Nếu không phải ApiSuccessResult, thử đọc trực tiếp
                        var list = JsonConvert.DeserializeObject<List<string>>(body, settings);
                        return list ?? new List<string>();
                    }
                    catch (JsonException)
                    {
                        // Nếu không deserialize được, trả về danh sách trống
                        Console.WriteLine($"Không thể deserialize dữ liệu ảnh, trả về danh sách trống: {body}");
                        return new List<string>();
                    }
                }
                
                Console.WriteLine($"API trả về lỗi khi lấy danh sách ảnh - Status: {response.StatusCode}, Body: {body}");
                return new List<string>(); // Trả về danh sách trống thay vì throw exception
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in GetListImages: {ex.Message}");
                return new List<string>(); // Trả về danh sách trống
            }
        }
    }
}
