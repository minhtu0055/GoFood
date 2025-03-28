using GoFood.ApiIntegration.Services;
using GoFood.Data.Enums;
using GoFood.ViewModels.Catalog.Orders;
using GoFood.ViewModels.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Globalization;

namespace GoFood.Admin.Controllers
{
    public class OrderController : BaseController
    {
        private readonly IOrderApiClient _orderApiClient;
        private readonly IUserApiClient _userApiClient;

        public OrderController(IOrderApiClient orderApiClient, IUserApiClient userApiClient)
        {
            _orderApiClient = orderApiClient;
            _userApiClient = userApiClient;
        }

        public async Task<IActionResult> Index(string keyword, Guid? userId, Status? status, string fromDate, string toDate, int pageIndex = 1, int pageSize = 10)
        {
            var request = new GetOrderPagingRequest()
            {
                Keyword = keyword,
                PageIndex = pageIndex,
                PageSize = pageSize,
                Status = status,
                UserId = userId
            };

            if (!string.IsNullOrEmpty(fromDate))
            {
                request.FromDate = DateTime.ParseExact(fromDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);
            }
            if (!string.IsNullOrEmpty(toDate))
            {
                request.ToDate = DateTime.ParseExact(toDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);
            }

            var data = await _orderApiClient.GetAllPaging(request);
            if (data == null)
            {
                data = new PagedResult<OrderViewModel>()
                {
                    Items = new List<OrderViewModel>(),
                    PageIndex = pageIndex,
                    PageSize = pageSize,
                    TotalRecords = 0
                };
            }

            ViewBag.Keyword = keyword;
            ViewBag.Status = status;
            ViewBag.FromDate = fromDate;
            ViewBag.ToDate = toDate;
            ViewBag.UserId = userId;

            return View(data);
        }
        [HttpGet]
        public async Task<IActionResult> Details(Guid id)
        {
            var result = await _orderApiClient.GetById(id);
            if (result != null)
            {
                return View(result);
            }
            return RedirectToAction("Error", "Home");
        }

        [HttpPost]
        public async Task<IActionResult> UpdateStatus(Guid id, Status status)
        {
            var request = new OrderStatusUpdateRequest()
            {
                Id = id,
                Status = status
            };
            
            var result = await _orderApiClient.UpdateStatus(id, request);
            if (result != null)
            {
                TempData["success"] = "Cập nhật trạng thái đơn hàng thành công";
                return RedirectToAction("Details", new { id = id });
            }

            TempData["error"] = "Failed to update order status";
            return RedirectToAction("Details", new { id = id });
        }

        [HttpPost]
        public async Task<IActionResult> CancelOrder(Guid id)
        {
            var result = await _orderApiClient.CancelOrder(id);
            if (result != null)
            {
                TempData["success"] = "Hủy đơn hàng thành công";
                return RedirectToAction("Details", new { id = id });
            }

            TempData["error"] = "Failed to cancel order";
            return RedirectToAction("Details", new { id = id });
        }

        [HttpGet]
        public async Task<IActionResult> ExportInvoice(Guid id)
        {
            var result = await _orderApiClient.GetById(id);
            if (result != null)
            {
                return View("Invoice", result);  // Changed from View(result) to View("Invoice", result)
            }
            return RedirectToAction("Error", "Home");
        }

        // Phương thức Index đã được định nghĩa ở trên

    }
}