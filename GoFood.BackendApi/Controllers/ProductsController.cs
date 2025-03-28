using GoFood.Application.Catalog.Products;
using GoFood.Data.Enums;
using GoFood.ViewModels.Catalog.Products;
using GoFood.ViewModels.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GoFood.BackendApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllPaging([FromQuery] GetProductPagingRequest request)
        {
            var products = await _productService.GetAllPaging(request);
            return Ok(new ApiSuccessResult<PagedResult<ProductViewModels>>(products));
        }

        [HttpGet("getall")]
        public async Task<IActionResult> GetAll()
        {
            var products = await _productService.GetAll();
            return Ok(new ApiSuccessResult<List<ProductViewModels>>(products));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var product = await _productService.GetById(id);
            if (product == null)
                return NotFound(new ApiErrorResult<ProductViewModels>($"Không tìm thấy sản phẩm có ID: {id}"));
                
            return Ok(new ApiSuccessResult<ProductViewModels>(product));
        }

        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Create([FromForm] ProductCreateRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiErrorResult<ProductViewModels>());

            var product = await _productService.Create(request);
            return CreatedAtAction(nameof(GetById), new { id = product.Id }, new ApiSuccessResult<ProductViewModels>(product));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] ProductUpdateRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiErrorResult<ProductViewModels>());

            request.Id = id;
            var product = await _productService.Update(request);
            return Ok(new ApiSuccessResult<ProductViewModels>(product));
        }

        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] ProductStatus status)
        {
            var result = await _productService.UpdateStatus(id, status);
            if (!result)
                return BadRequest(new ApiErrorResult<bool>("Cập nhật trạng thái không thành công"));
                
            return Ok(new ApiSuccessResult<bool>(true));
        }

        [HttpPatch("{id}/price")]
        public async Task<IActionResult> UpdatePrice(Guid id, [FromBody] decimal newPrice)
        {
            var result = await _productService.UpdatePrice(id, newPrice);
            if (!result)
                return BadRequest(new ApiErrorResult<bool>("Cập nhật giá không thành công"));
                
            return Ok(new ApiSuccessResult<bool>(true));
        }

        [HttpPatch("{id}/stock")]
        public async Task<IActionResult> UpdateStock(Guid id, [FromBody] int addedQuantity)
        {
            var result = await _productService.UpdateStock(id, addedQuantity);
            if (!result)
                return BadRequest(new ApiErrorResult<bool>("Cập nhật số lượng không thành công"));
                
            return Ok(new ApiSuccessResult<bool>(true));
        }

        [HttpPost("{id}/images")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> AddImage(Guid id, IFormFile file)
        {
            try
            {
                Console.WriteLine($"[API] AddImage called for product {id}, file: {(file != null ? $"{file.FileName}, {file.Length} bytes" : "null")}");
                
                if (file == null)
                {
                    Console.WriteLine("[API] AddImage failed - file is null");
                    return BadRequest(new ApiErrorResult<int>("File không được để trống"));
                }

                // Kiểm tra kích thước file (2MB)
                if (file.Length > 2 * 1024 * 1024)
                {
                    Console.WriteLine($"[API] AddImage failed - file size {file.Length} exceeds limit");
                    return BadRequest(new ApiErrorResult<int>("Kích thước ảnh không được vượt quá 2MB"));
                }

                // Kiểm tra định dạng file
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!allowedExtensions.Contains(extension))
                {
                    Console.WriteLine($"[API] AddImage failed - invalid file extension: {extension}");
                    return BadRequest(new ApiErrorResult<int>("Chỉ chấp nhận file ảnh có định dạng .jpg, .jpeg, .png, .gif"));
                }
                
                Console.WriteLine("[API] AddImage validation passed, calling service");
                var result = await _productService.AddImage(id, file);
                
                Console.WriteLine($"[API] AddImage service result: {result}");
                if (result == 0)
                    return BadRequest(new ApiErrorResult<int>("Thêm ảnh không thành công"));
                    
                return Ok(new ApiSuccessResult<int>(result));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[API] AddImage exception: {ex.Message}");
                Console.WriteLine($"[API] Stack trace: {ex.StackTrace}");
                return StatusCode(500, new ApiErrorResult<int>($"Lỗi server: {ex.Message}"));
            }
        }

        [HttpDelete("images/{imageId}")]
        public async Task<IActionResult> RemoveImage(Guid imageId)
        {
            try
            {
                Console.WriteLine($"[API] RemoveImage called for image {imageId}");
                
                var result = await _productService.RemoveImage(imageId);
                
                Console.WriteLine($"[API] RemoveImage service result: {result}");
                if (result == 0)
                {
                    Console.WriteLine($"[API] RemoveImage failed - image not found or could not be deleted");
                    return BadRequest(new ApiErrorResult<int>("Xóa ảnh không thành công - ảnh không tồn tại hoặc không thể xóa"));
                }
                
                Console.WriteLine("[API] RemoveImage successful");
                return Ok(new ApiSuccessResult<int>(result));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[API] RemoveImage exception: {ex.Message}");
                Console.WriteLine($"[API] Stack trace: {ex.StackTrace}");
                return StatusCode(500, new ApiErrorResult<int>($"Lỗi server: {ex.Message}"));
            }
        }

        [HttpGet("{id}/images")]
        public async Task<IActionResult> GetListImages(Guid id)
        {
            try
            {
                Console.WriteLine($"[API] GetListImages called for product {id}");
                
                var images = await _productService.GetListImages(id);
                
                Console.WriteLine($"[API] GetListImages returned {images?.Count ?? 0} images");
                return Ok(new ApiSuccessResult<List<string>>(images));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[API] GetListImages exception: {ex.Message}");
                Console.WriteLine($"[API] Stack trace: {ex.StackTrace}");
                return StatusCode(500, new ApiErrorResult<List<string>>($"Lỗi server: {ex.Message}"));
            }
        }
    }
}
