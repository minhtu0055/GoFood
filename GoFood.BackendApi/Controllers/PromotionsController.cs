using System;
using System.Threading.Tasks;
using GoFood.Application.Catalog.Promotions;
using GoFood.Data.Enums;
using GoFood.ViewModels.Catalog.Promotions;
using GoFood.ViewModels.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GoFood.BackendApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize]
    public class PromotionsController : ControllerBase
    {
        private readonly IPromotionService _promotionService;

        public PromotionsController(IPromotionService promotionService)
        {
            _promotionService = promotionService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllPaging([FromQuery] GetPromotionPagingRequest request)
        {
            var promotions = await _promotionService.GetAllPaging(request);
            return Ok(new ApiSuccessResult<PagedResult<PromotionViewModel>>(promotions));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var promotion = await _promotionService.GetById(id);
            if (promotion == null)
                return NotFound(new ApiErrorResult<PromotionViewModel>($"Không tìm thấy khuyến mãi có ID: {id}"));
                
            return Ok(new ApiSuccessResult<PromotionViewModel>(promotion));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreatePromotionRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiErrorResult<PromotionViewModel>());

            var promotion = await _promotionService.Create(request);
            return CreatedAtAction(nameof(GetById), new { id = promotion.Id }, new ApiSuccessResult<PromotionViewModel>(promotion));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePromotionRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiErrorResult<PromotionViewModel>());

            request.Id = id;
            var promotion = await _promotionService.Update(request);
            return Ok(new ApiSuccessResult<PromotionViewModel>(promotion));
        }

        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] PromotionStatus status)
        {
            var result = await _promotionService.UpdateStatus(id, status);
            if (!result)
                return BadRequest(new ApiErrorResult<bool>("Cập nhật trạng thái không thành công"));
                
            return Ok(new ApiSuccessResult<bool>(true));
        }
    }
} 