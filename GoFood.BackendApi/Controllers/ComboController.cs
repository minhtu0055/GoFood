using GoFood.Application.Catalog.Combo;
using GoFood.ViewModels.Catalog.Combo;
using GoFood.ViewModels.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GoFood.BackendApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ComboController : ControllerBase
    {
        private readonly IComboService _comboService;

        public ComboController(IComboService comboService)
        {
            _comboService = comboService;
        }

        [HttpGet("paging")]
        public async Task<IActionResult> GetAllPaging([FromQuery] GetComboPagingRequest request)
        {
            var combos = await _comboService.GetAllPaging(request);
            return Ok(new ApiSuccessResult<PagedResult<ComboViewModel>>(combos));
        }

        [HttpGet("getall")]
        public async Task<IActionResult> GetAll()
        {
            var combos = await _comboService.GetAll();
            return Ok(new ApiSuccessResult<List<ComboViewModel>>(combos));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var combo = await _comboService.GetById(id);
            if (combo == null)
                return NotFound(new ApiErrorResult<ComboViewModel>($"Không tìm thấy combo có ID: {id}"));
                
            return Ok(new ApiSuccessResult<ComboViewModel>(combo));
        }

        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Create([FromForm] ComboCreateRequest request)
        {
            var result = await _comboService.Create(request);
            if (result == null)
                return BadRequest(new ApiErrorResult<ComboViewModel>("Tạo combo thất bại"));

            return Ok(new ApiSuccessResult<ComboViewModel>(result));
        }

        [HttpPut]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Update([FromForm] ComboUpdateRequest request)
        {
            var result = await _comboService.Update(request);
            if (result == null)
                return NotFound(new ApiErrorResult<ComboViewModel>($"Không tìm thấy combo có ID: {request.Id}"));

            return Ok(new ApiSuccessResult<ComboViewModel>(result));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _comboService.Delete(id);
            if (!result)
                return NotFound(new ApiErrorResult<bool>($"Không tìm thấy combo có ID: {id}"));

            return Ok(new ApiSuccessResult<bool>(result));
        }

        [HttpPut("{id}/availability")]
        public async Task<IActionResult> UpdateAvailability(Guid id, [FromBody] bool isAvailable)
        {
            var result = await _comboService.UpdateAvailability(id, isAvailable);
            if (!result)
                return NotFound(new ApiErrorResult<bool>($"Không tìm thấy combo có ID: {id}"));

            return Ok(new ApiSuccessResult<bool>(result));
        }
    }
} 