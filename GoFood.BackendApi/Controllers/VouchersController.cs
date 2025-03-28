using System;
using System.Threading.Tasks;
using GoFood.Application.Catalog.Vouchers;
using GoFood.Data.Enums;
using GoFood.ViewModels.Catalog.Vouchers;
using GoFood.ViewModels.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GoFood.BackendApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class VouchersController : ControllerBase
    {
        private readonly IVoucherService _voucherService;

        public VouchersController(IVoucherService voucherService)
        {
            _voucherService = voucherService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllPaging([FromQuery] GetVoucherPagingRequest request)
        {
            var vouchers = await _voucherService.GetAllPaging(request);
            return Ok(new ApiSuccessResult<PagedResult<VoucherViewModel>>(vouchers));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var voucher = await _voucherService.GetById(id);
            if (voucher == null)
                return NotFound(new ApiErrorResult<VoucherViewModel>($"Không tìm thấy voucher có ID: {id}"));
                
            return Ok(new ApiSuccessResult<VoucherViewModel>(voucher));
        }

        [HttpGet("code/{code}")]
        public async Task<IActionResult> GetByCode(string code)
        {
            var voucher = await _voucherService.GetByCode(code);
            if (voucher == null)
                return NotFound(new ApiErrorResult<VoucherViewModel>($"Không tìm thấy voucher có mã: {code}"));
                
            return Ok(new ApiSuccessResult<VoucherViewModel>(voucher));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateVoucherRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiErrorResult<VoucherViewModel>());

            var voucher = await _voucherService.Create(request);
            return CreatedAtAction(nameof(GetById), new { id = voucher.Id }, new ApiSuccessResult<VoucherViewModel>(voucher));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateVoucherRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiErrorResult<VoucherViewModel>());

            request.Id = id;
            var voucher = await _voucherService.Update(request);
            return Ok(new ApiSuccessResult<VoucherViewModel>(voucher));
        }
        [HttpPost("code/{code}/use")]

        public async Task<IActionResult> UseVoucher(string code)
        {
            var result = await _voucherService.UseVoucher(code);
            if (!result)
                return BadRequest(new ApiErrorResult<bool>("Sử dụng voucher không thành công"));
                
            return Ok(new ApiSuccessResult<bool>(true));
        }

        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] PromotionStatus status)
        {
            var result = await _voucherService.UpdateStatus(id, status);
            if (!result)
                return BadRequest(new ApiErrorResult<bool>("Cập nhật trạng thái voucher thất bại"));
                
            return Ok(new ApiSuccessResult<bool>(true));
        }
    }
} 