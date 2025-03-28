using GoFood.ViewModels.Catalog.Products;
using GoFood.ViewModels.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using GoFood.Data.Enums;
using GoFood.ApiIntegration.Services;


namespace GoFood.Admin.Controllers
{
    public class ProductController : BaseController
    {
        private readonly IProductApiClient _productApiClient;
        private readonly ICategoryApiClient _categoryApiClient;
        private readonly IConfiguration _configuration;

        public ProductController(IProductApiClient productApiClient,
            ICategoryApiClient categoryApiClient,
            IConfiguration configuration)
        {
            _productApiClient = productApiClient;
            _categoryApiClient = categoryApiClient;
            _configuration = configuration;
        }

        public async Task<IActionResult> Index(string? keyword, Guid? categoryId, decimal? minPrice, decimal? maxPrice, int pageIndex = 1, int pageSize = 5)
        {
            var request = new GetProductPagingRequest()
            {
                Keyword = keyword,
                CategoryId = categoryId,
                MinPrice = minPrice,
                MaxPrice = maxPrice,
                PageIndex = pageIndex,
                PageSize = pageSize
            };

            var data = await _productApiClient.GetAllPaging(request);
            var categories = await _categoryApiClient.GetAll();

            ViewBag.Categories = categories.Select(x => new SelectListItem()
            {
                Text = x.Name,
                Value = x.Id.ToString(),
                Selected = categoryId.HasValue && categoryId.Value == x.Id
            });

            ViewBag.Keyword = keyword;
            ViewBag.CategoryId = categoryId;
            ViewBag.MinPrice = minPrice;
            ViewBag.MaxPrice = maxPrice;

            return View(data);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var categories = await _categoryApiClient.GetAll();
            ViewBag.Categories = categories.Select(x => new SelectListItem()
            {
                Text = x.Name,
                Value = x.Id.ToString()
            });
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductCreateRequest request)
        {
            if (!ModelState.IsValid)
            {
                var categories = await _categoryApiClient.GetAll();
                ViewBag.Categories = categories.Select(x => new SelectListItem()
                {
                    Text = x.Name,
                    Value = x.Id.ToString()
                });
                return View(request);
            }

            var result = await _productApiClient.Create(request);
            if (result != null)
            {
                TempData["result"] = "Thêm mới sản phẩm thành công";
                return RedirectToAction("Index");
            }

            ModelState.AddModelError("", "Thêm sản phẩm thất bại");
            return View(request);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            try
            {
                var product = await _productApiClient.GetById(id);
                if (product == null)
                {
                    TempData["error"] = "Không tìm thấy sản phẩm";
                    return RedirectToAction("Index");
                }

                var categories = await _categoryApiClient.GetAll();
                ViewBag.Categories = categories.Select(x => new SelectListItem()
                {
                    Text = x.Name,
                    Value = x.Id.ToString(),
                    Selected = x.Id == product.CategoryId
                });

                // Lấy danh sách ảnh của sản phẩm
                var images = await _productApiClient.GetListImages(id);

                Console.WriteLine($"[Edit] Danh sách ảnh từ API: {string.Join(", ", images)}");

                // Tạo model update request
                var updateRequest = new ProductUpdateRequest()
                {
                    Id = product.Id,
                    Name = product.Name,
                    Description = product.Description,
                    CategoryId = product.CategoryId,
                    Price = product.Price,
                    Quantity = product.Quantity,
                    ProductImages = new List<ProductImageViewModel>()
                };

                // Nếu có ảnh, chuyển đổi từ URL sang model ProductImageViewModel
                if (images != null && images.Count > 0)
                {
                    var baseUrl = _configuration["BaseAddress"]?.TrimEnd('/');
                    Console.WriteLine($"[Edit] BaseUrl: {baseUrl}");

                    foreach (var imageUrl in images)
                    {
                        // Đảm bảo imageUrl không null
                        if (string.IsNullOrEmpty(imageUrl))
                        {
                            Console.WriteLine("[Edit] Bỏ qua URL ảnh null hoặc trống");
                            continue;
                        }

                        // Log chi tiết thông tin imageUrl
                        Console.WriteLine($"[Edit] Xử lý ảnh gốc: {imageUrl}");

                        // Lấy tên file từ đường dẫn
                        string fileName = imageUrl.Split('/').LastOrDefault();
                        Console.WriteLine($"[Edit] Tên file: {fileName}");

                        if (string.IsNullOrEmpty(fileName))
                        {
                            Console.WriteLine("[Edit] Không thể tách tên file, bỏ qua ảnh này");
                            continue;
                        }

                        // Lấy ID ảnh từ tên file (đây là GUID)
                        string imageId = Path.GetFileNameWithoutExtension(fileName);
                        Console.WriteLine($"[Edit] ImageId từ tên file: {imageId}");

                        // Kiểm tra ID có đúng định dạng GUID không
                        if (Guid.TryParse(imageId, out Guid guidId))
                        {
                            // Xây dựng URL đầy đủ
                            string fullImageUrl;

                            // Kiểm tra xem imageUrl đã là URL đầy đủ chưa
                            if (imageUrl.StartsWith("http"))
                            {
                                // Đã là URL đầy đủ
                                fullImageUrl = imageUrl;
                                Console.WriteLine($"[Edit] Đã là URL đầy đủ: {fullImageUrl}");
                            }
                            else
                            {
                                // Xử lý các trường hợp khác nhau của imageUrl
                                if (imageUrl.StartsWith("/"))
                                {
                                    // URL bắt đầu bằng dấu / - ghép trực tiếp với baseUrl
                                    fullImageUrl = $"{baseUrl}{imageUrl}";
                                    Console.WriteLine($"[Edit] URL bắt đầu bằng / nên ghép trực tiếp: {fullImageUrl}");
                                }
                                else
                                {
                                    // URL không bắt đầu bằng / - thêm / vào giữa
                                    fullImageUrl = $"{baseUrl}/{imageUrl}";
                                    Console.WriteLine($"[Edit] URL không bắt đầu bằng / nên thêm / vào giữa: {fullImageUrl}");
                                }
                            }

                            // Thêm vào danh sách ảnh
                            updateRequest.ProductImages.Add(new ProductImageViewModel
                            {
                                Id = guidId,
                                ImageUrl = fullImageUrl
                            });

                            Console.WriteLine($"[Edit] Đã thêm ảnh với Id={guidId}, URL={fullImageUrl}");
                        }
                        else
                        {
                            Console.WriteLine($"[Edit] Không thể parse ImageId thành Guid: {imageId}");
                        }
                    }
                }

                // In thông tin ảnh đã xử lý
                if (updateRequest.ProductImages.Any())
                {
                    Console.WriteLine($"[Edit] Số lượng ảnh đã xử lý: {updateRequest.ProductImages.Count}");
                    foreach (var img in updateRequest.ProductImages)
                    {
                        Console.WriteLine($"[Edit] Ảnh đã xử lý - Id: {img.Id}, Url: {img.ImageUrl}");
                    }
                }
                else
                {
                    Console.WriteLine("[Edit] Không có ảnh nào được xử lý");
                }

                // Kiểm tra và hiển thị cảnh báo nếu không có ảnh
                if (updateRequest.ProductImages == null || !updateRequest.ProductImages.Any())
                {
                    TempData["warning"] = "Sản phẩm này chưa có ảnh, vui lòng thêm ít nhất một ảnh.";
                }

                return View(updateRequest);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in Edit GET: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                TempData["error"] = $"Lỗi: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProductUpdateRequest request)
        {
            if (!ModelState.IsValid)
            {
                var categories = await _categoryApiClient.GetAll();
                ViewBag.Categories = categories.Select(x => new SelectListItem()
                {
                    Text = x.Name,
                    Value = x.Id.ToString(),
                    Selected = x.Id == request.CategoryId
                });

                // Lấy lại danh sách ảnh nếu form không hợp lệ
                var images = await _productApiClient.GetListImages(request.Id);
                ViewBag.Images = images;

                return View(request);
            }

            try
            {
                var result = await _productApiClient.Update(request);
                TempData["result"] = "Cập nhật sản phẩm thành công";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(request);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var result = await _productApiClient.UpdateStatus(id, ProductStatus.InActive);
                TempData["result"] = "Vô hiệu hóa sản phẩm thành công";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Active(Guid id)
        {
            try
            {
                await _productApiClient.UpdateStatus(id, ProductStatus.Active);
                TempData["result"] = "Kích hoạt sản phẩm thành công";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
                return RedirectToAction("Index");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Details(Guid id)
        {
            try
            {
                var product = await _productApiClient.GetById(id);
                return View(product);
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddImage(Guid productId, IFormFile image)
        {
            try
            {
                // Kiểm tra đầu vào cơ bản
                if (image == null || image.Length == 0)
                {
                    return Json(new { success = false, message = "Vui lòng chọn ảnh" });
                }

                // Kiểm tra kích thước file (2MB)
                if (image.Length > 2 * 1024 * 1024)
                {
                    return Json(new { success = false, message = "Kích thước ảnh không được vượt quá 2MB" });
                }

                // Kiểm tra định dạng file
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var extension = Path.GetExtension(image.FileName).ToLowerInvariant();
                if (!allowedExtensions.Contains(extension))
                {
                    return Json(new { success = false, message = "Chỉ chấp nhận file ảnh có định dạng .jpg, .jpeg, .png, .gif" });
                }

                // Gọi API
                var result = await _productApiClient.AddImage(productId, image);

                if (result > 0)
                {
                    // Lấy danh sách ảnh mới
                    var images = await _productApiClient.GetListImages(productId);

                    if (images == null || !images.Any())
                    {
                        return Json(new
                        {
                            success = true,
                            message = "Thêm ảnh thành công nhưng không thể hiển thị",
                            images = new List<string>()
                        });
                    }

                    // Xử lý URL ảnh
                    var baseUrl = _configuration["BaseAddress"]?.TrimEnd('/');
                    var imageUrls = new List<string>();

                    foreach (var img in images)
                    {
                        if (string.IsNullOrEmpty(img)) continue;

                        string fullUrl;
                        if (img.StartsWith("http"))
                        {
                            fullUrl = img; // Đã là URL đầy đủ
                        }
                        else
                        {
                            // Ghép URL
                            fullUrl = img.StartsWith("/")
                                ? $"{baseUrl}{img}"
                                : $"{baseUrl}/{img}";
                        }

                        imageUrls.Add(fullUrl);
                    }

                    // Trả về ảnh mới nhất
                    var latestImage = imageUrls.LastOrDefault();
                    if (latestImage != null)
                    {
                        return Json(new
                        {
                            success = true,
                            message = "Thêm ảnh thành công",
                            images = new List<string> { latestImage }
                        });
                    }

                    return Json(new
                    {
                        success = true,
                        message = "Thêm ảnh thành công",
                        images = imageUrls
                    });
                }

                return Json(new { success = false, message = "Thêm ảnh thất bại" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> RemoveImage(string imageId)
        {
            if (string.IsNullOrEmpty(imageId))
            {
                return Json(new { success = false, message = "ID ảnh không được để trống" });
            }

            try
            {
                // Chỉ xử lý nếu imageId là Guid hợp lệ
                if (Guid.TryParse(imageId, out Guid guidId))
                {
                    var result = await _productApiClient.RemoveImage(guidId);
                    if (result > 0)
                    {
                        return Json(new { success = true, message = "Xóa ảnh thành công" });
                    }
                    return Json(new { success = false, message = "Xóa ảnh thất bại" });
                }
                else
                {
                    return Json(new { success = false, message = "ID ảnh không đúng định dạng" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }
    }
}
