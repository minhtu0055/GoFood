using GoFood.Application.Catalog.Orders;
using GoFood.ViewModels.Catalog.Combo;
using GoFood.ViewModels.Catalog.Orders;
using GoFood.ViewModels.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GoFood.BackendApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrdersController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpGet("paging")]
        public async Task<IActionResult> GetAllPaging([FromQuery] GetOrderPagingRequest request)
        {
            var orders = await _orderService.GetAllPaging(request);
            return Ok(new ApiSuccessResult<PagedResult<OrderViewModel>>(orders));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var order = await _orderService.GetById(id);
                if (order == null)
                    return NotFound(new ApiErrorResult<OrderViewModel>($"Không tìm thấy đơn hàng với ID: {id}"));

                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var isAdmin = User.IsInRole("Admin");
                string orderUserIdString = order.UserId.ToString();

                if (!isAdmin && orderUserIdString != userId)
                    return Forbid();

                return Ok(new ApiSuccessResult<OrderViewModel>(order));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiErrorResult<OrderViewModel>($"Lỗi khi lấy thông tin đơn hàng: {ex.Message}"));
            }
        }

        [HttpGet("user")]
        public async Task<IActionResult> GetOrdersByUser()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new ApiErrorResult<List<OrderViewModel>>("Unauthorized"));

            var orders = await _orderService.GetOrdersByUserId(Guid.Parse(userId));
            return Ok(new ApiSuccessResult<List<OrderViewModel>>(orders));
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetOrdersByUserId(Guid userId)
        {
            var orders = await _orderService.GetOrdersByUserId(userId);
            return Ok(new ApiSuccessResult<List<OrderViewModel>>(orders));
        }

        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] OrderStatusUpdateRequest request)
        {
            if (!ModelState.IsValid)
            return BadRequest(new ApiErrorResult<bool>("Trạng thái không hợp lệ"));

            request.Id = id;
            var result = await _orderService.UpdateStatus(request);
            if (!result)
                return BadRequest(new ApiErrorResult<bool>("Không thể cập nhật trạng thái đơn hàng"));

            return Ok(new ApiSuccessResult<bool>(result));
        }

        [HttpPost("{id}/cancel")]
        public async Task<IActionResult> CancelOrder(Guid id)
        {
            var order = await _orderService.GetById(id);
            if (order == null)
                return NotFound(new ApiErrorResult<bool>($"Không tìm thấy đơn hàng với ID: {id}"));

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var isAdmin = User.IsInRole("admin");

            if (!isAdmin && order.UserId.ToString() != userId)
                return Forbid();

            var result = await _orderService.CancelOrder(id);
            if (!result)
                return BadRequest(new ApiErrorResult<bool>("Không thể hủy đơn hàng"));

            return Ok(new ApiSuccessResult<bool>(result));
        }

        [HttpGet("{id}/invoice")]
        public async Task<IActionResult> ExportInvoice(Guid id)
        {
            var order = await _orderService.GetById(id);
            if (order == null)
                return NotFound(new ApiErrorResult<OrderViewModel>($"Không tìm thấy đơn hàng với ID: {id}"));

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var isAdmin = User.IsInRole("admin");

            if (!isAdmin && order.UserId.ToString() != userId)
                return Forbid();

            var invoice = await _orderService.ExportOrderInvoice(id);
            return Ok(new ApiSuccessResult<OrderViewModel>(invoice));
        }
    }
}