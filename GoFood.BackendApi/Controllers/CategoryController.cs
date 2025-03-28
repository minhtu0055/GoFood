using GoFood.Application.Catalog.Category;
using GoFood.ViewModels.Catalog.Category;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GoFood.BackendApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }
        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
        {
            var categories = await _categoryService.GetAll();
            return Ok(categories);
        }
        [HttpGet("GetById/{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var category = await _categoryService.GetById(id);
            return Ok(category);
        }
        [HttpPost("Create")]
        public async Task<IActionResult> Create([FromBody] CategoryCreateRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var category = await _categoryService.Create(request);
            return CreatedAtAction(nameof(GetById), new { id = category.Id }, category);
        }

        [HttpPut("Edit/{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] CategoryUpdateRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            request.Id = id;
            var category = await _categoryService.Update(request);
            return Ok(category);
        }

        [HttpDelete("Delete/{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _categoryService.Delete(id);
            if (!result)
                return BadRequest("Không thể xóa danh mục");
            return Ok();
        }
    }
}
