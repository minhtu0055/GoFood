using System;
using System.Threading.Tasks;
using GoFood.ApiIntegration.Services;
using GoFood.ViewModels.Catalog.Combo;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GoFood.Admin.Controllers
{
    public class ComboController : BaseController
    {
        private readonly IComboApiClient _comboApiClient;
        private readonly IProductApiClient _productApiClient;

        public ComboController(IComboApiClient comboApiClient, IProductApiClient productApiClient)
        {
            _comboApiClient = comboApiClient;
            _productApiClient = productApiClient;
        }

        public async Task<IActionResult> Index(string? keyword, decimal? minPrice, decimal? maxPrice, string sortBy, int pageIndex = 1, int pageSize = 10)
        {
            try
            {
                var request = new GetComboPagingRequest()
                {
                    Keyword = keyword,
                    PageIndex = pageIndex,
                    PageSize = pageSize,
                    MinPrice = minPrice,
                    MaxPrice = maxPrice,
                    SortBy = sortBy
                };
                var data = await _comboApiClient.GetAllPaging(request);
                ViewBag.Keyword = keyword;
                ViewBag.MinPrice = minPrice;
                ViewBag.MaxPrice = maxPrice;
                ViewBag.SortBy = sortBy;
                if (TempData["result"] != null)
                {
                    ViewBag.SuccessMsg = TempData["result"];
                }
                return View(data);
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
                return RedirectToAction("Error", "Home");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            try
            {
                var products = await _productApiClient.GetAll();
                ViewBag.Products = products.Select(x => new SelectListItem()
                {
                    Text = x.Name,
                    Value = x.Id.ToString()
                });
                return View();
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
                return RedirectToAction("Error", "Home");
            }
        }

        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Create([FromForm] ComboCreateRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var products = await _productApiClient.GetAll();
                    ViewBag.Products = products.Select(x => new SelectListItem()
                    {
                        Text = x.Name,
                        Value = x.Id.ToString()
                    });
                    return View(request);
                }

                var result = await _comboApiClient.Create(request);
                TempData["result"] = "Thêm mới combo thành công";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                var products = await _productApiClient.GetAll();
                ViewBag.Products = products.Select(x => new SelectListItem()
                {
                    Text = x.Name,
                    Value = x.Id.ToString()
                });
                return View(request);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            try
            {
                var combo = await _comboApiClient.GetById(id);
                if (combo == null)
                {
                    TempData["error"] = "Không tìm thấy combo";
                    return RedirectToAction("Index");
                }

                // Đảm bảo tất cả các thuộc tính được gán giá trị
                var updateRequest = new ComboUpdateRequest()
                {
                    Id = id,
                    Name = combo.Name,
                    Description = combo.Description,
                    Price = combo.Price,
                    IsAvailable = combo.IsAvailable,
                    ProductIds = combo.ComboProducts?.Select(x => x.ProductId).ToList() ?? new List<Guid>(),
                    Quantities = combo.ComboProducts?.Select(x => x.Quantity).ToList() ?? new List<int>()
                };

                var products = await _productApiClient.GetAll();
                ViewBag.Products = products.Select(x => new SelectListItem()
                {
                    Text = x.Name,
                    Value = x.Id.ToString(),
                    // Thêm Selected để đánh dấu các sản phẩm đã được chọn
                    Selected = updateRequest.ProductIds.Contains(x.Id)
                });

                return View(updateRequest);
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
                return RedirectToAction("Error", "Home");
            }
        }

        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Edit([FromForm] ComboUpdateRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var products = await _productApiClient.GetAll();
                    ViewBag.Products = products.Select(x => new SelectListItem()
                    {
                        Text = x.Name,
                        Value = x.Id.ToString()
                    });
                    return View(request);
                }

                var result = await _comboApiClient.Update(request);
                TempData["result"] = "Cập nhật combo thành công";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                var products = await _productApiClient.GetAll();
                ViewBag.Products = products.Select(x => new SelectListItem()
                {
                    Text = x.Name,
                    Value = x.Id.ToString()
                });
                return View(request);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                await _comboApiClient.Delete(id);
                TempData["result"] = "Xóa combo thành công";
                return Json(new { success = true, message = "Xóa combo thành công" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateAvailability(Guid id, bool isAvailable)
        {
            try
            {
                await _comboApiClient.UpdateAvailability(id, isAvailable);
                return Json(new { success = true, message = "Cập nhật trạng thái thành công" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}