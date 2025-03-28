using GoFood.ApiIntegration.Services;
using GoFood.Data.Enums;
using GoFood.ViewModels.Catalog.Promotions;
using GoFood.ViewModels.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GoFood.Admin.Controllers
{
    public class PromotionController : BaseController
    {
        private readonly IPromotionApiClient _promotionApiClient;
        private readonly IProductApiClient _productApiClient;

        public PromotionController(IPromotionApiClient promotionApiClient, IProductApiClient productApiClient)
        {
            _promotionApiClient = promotionApiClient;
            _productApiClient = productApiClient;
        }

        public async Task<IActionResult> Index(string keyword, PromotionStatus? status, int pageIndex = 1, int pageSize = 10)
        {
            var request = new GetPromotionPagingRequest()
            {
                Keyword = keyword,
                Status = status,
                PageIndex = pageIndex,
                PageSize = pageSize
            };

            var data = await _promotionApiClient.GetAllPaging(request);

            ViewBag.Keyword = keyword;
            ViewBag.Status = status;

            ViewBag.StatusSelectList = new List<SelectListItem>()
            {
                new SelectListItem("Tất cả", "", !status.HasValue),
                new SelectListItem("Không hoạt động", PromotionStatus.Inactive.ToString(), status == PromotionStatus.Inactive),
                new SelectListItem("Đang hoạt động", PromotionStatus.Active.ToString(), status == PromotionStatus.Active),
                new SelectListItem("Đã hết hạn", PromotionStatus.Expired.ToString(), status == PromotionStatus.Expired)
            };

            return View(data);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var products = await _productApiClient.GetAll();
            // Đảm bảo danh sách hình ảnh không null
            foreach (var product in products)
            {
                product.Images = product.Images ?? new List<string>();
            }
            ViewBag.Products = products;

            ViewBag.DiscountTypes = new List<SelectListItem>()
            {
                new SelectListItem("Giảm theo phần trăm", ((int)DiscountType.Percentage).ToString()),
                new SelectListItem("Giảm theo số tiền", ((int)DiscountType.Amount).ToString())
            };

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreatePromotionRequest request)
        {
            if (!ModelState.IsValid)
            {
                var products = await _productApiClient.GetAll();
                // Đảm bảo danh sách hình ảnh không null
                foreach (var product in products)
                {
                    product.Images = product.Images ?? new List<string>();
                }
                ViewBag.Products = products;

                ViewBag.DiscountTypes = new List<SelectListItem>()
                {
                    new SelectListItem("Giảm theo phần trăm", ((int)DiscountType.Percentage).ToString()),
                    new SelectListItem("Giảm theo số tiền", ((int)DiscountType.Amount).ToString())
                };

                return View(request);
            }

            try
            {
                var result = await _promotionApiClient.Create(request);
                TempData["SuccessMsg"] = "Tạo khuyến mãi thành công";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                var products = await _productApiClient.GetAll();
                // Đảm bảo danh sách hình ảnh không null
                foreach (var product in products)
                {
                    product.Images = product.Images ?? new List<string>();
                }
                ViewBag.Products = products;

                ViewBag.DiscountTypes = new List<SelectListItem>()
                {
                    new SelectListItem("Giảm theo phần trăm", ((int)DiscountType.Percentage).ToString()),
                    new SelectListItem("Giảm theo số tiền", ((int)DiscountType.Amount).ToString())
                };

                return View(request);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            var promotion = await _promotionApiClient.GetById(id);
            if (promotion == null)
                return NotFound();

            var updateRequest = new UpdatePromotionRequest()
            {
                Id = promotion.Id,
                Name = promotion.Name,
                Description = promotion.Description,
                StartDate = promotion.StartDate,
                EndDate = promotion.EndDate,
                DiscountType = promotion.DiscountType,
                DiscountValue = promotion.DiscountValue,
                ProductIds = promotion.Products?.Select(p => p.ProductId).ToList() ?? new List<Guid>()
            };

            var products = await _productApiClient.GetAll();
            // Đảm bảo danh sách hình ảnh không null
            foreach (var product in products)
            {
                product.Images = product.Images ?? new List<string>();
            }
            ViewBag.Products = products;

            ViewBag.DiscountTypes = new List<SelectListItem>()
            {
                new SelectListItem("Giảm theo phần trăm", DiscountType.Percentage.ToString("d"), promotion.DiscountType == DiscountType.Percentage),
                new SelectListItem("Giảm theo số tiền", DiscountType.Amount.ToString("d"), promotion.DiscountType == DiscountType.Amount)
            };

            return View(updateRequest);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(UpdatePromotionRequest request)
        {
            if (!ModelState.IsValid)
            {
                var products = await _productApiClient.GetAll();
                // Đảm bảo danh sách hình ảnh không null
                foreach (var product in products)
                {
                    product.Images = product.Images ?? new List<string>();
                }
                ViewBag.Products = products;

                ViewBag.DiscountTypes = new List<SelectListItem>()
                {
                    new SelectListItem("Giảm theo phần trăm", DiscountType.Percentage.ToString("d"), request.DiscountType == DiscountType.Percentage),
                    new SelectListItem("Giảm theo số tiền", DiscountType.Amount.ToString("d"), request.DiscountType == DiscountType.Amount)
                };

                return View(request);
            }

            try
            {
                var result = await _promotionApiClient.Update(request);
                TempData["SuccessMsg"] = "Cập nhật khuyến mãi thành công";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                var products = await _productApiClient.GetAll();
                // Đảm bảo danh sách hình ảnh không null
                foreach (var product in products)
                {
                    product.Images = product.Images ?? new List<string>();
                }
                ViewBag.Products = products;

                ViewBag.DiscountTypes = new List<SelectListItem>()
                {
                    new SelectListItem("Giảm theo phần trăm", DiscountType.Percentage.ToString("d"), request.DiscountType == DiscountType.Percentage),
                    new SelectListItem("Giảm theo số tiền", DiscountType.Amount.ToString("d"), request.DiscountType == DiscountType.Amount)
                };

                return View(request);
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateStatus(Guid id, PromotionStatus status)
        {
            try
            {
                var result = await _promotionApiClient.UpdateStatus(id, status);
                if (result)
                {
                    TempData["SuccessMsg"] = "Cập nhật trạng thái khuyến mãi thành công";
                }
                else
                {
                    TempData["ErrorMsg"] = "Cập nhật trạng thái khuyến mãi thất bại";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMsg"] = ex.Message;
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Details(Guid id)
        {
            var promotion = await _promotionApiClient.GetById(id);
            if (promotion == null)
                return NotFound();

            // Lấy danh sách sản phẩm để cập nhật hình ảnh
            var products = await _productApiClient.GetAll();
            
            // Cập nhật đường dẫn hình ảnh cho từng sản phẩm trong promotion
            if (promotion.Products != null)
            {
                foreach (var promotionProduct in promotion.Products)
                {
                    var product = products.FirstOrDefault(p => p.Id == promotionProduct.ProductId);
                    if (product != null && product.Images != null && product.Images.Any())
                    {
                        promotionProduct.ProductImage = product.Images.First();
                    }
                }
            }

            return View(promotion);
        }
    }
} 