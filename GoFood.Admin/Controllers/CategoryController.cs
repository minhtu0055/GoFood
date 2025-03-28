using GoFood.ApiIntegration.Services;
using GoFood.ViewModels.Catalog.Category;
using Microsoft.AspNetCore.Mvc;

namespace GoFood.Admin.Controllers
{
    public class CategoryController : Controller
    {
        private readonly ICategoryApiClient _categoryApiClient;

        public CategoryController(ICategoryApiClient categoryApiClient)
        {
            _categoryApiClient = categoryApiClient;
        }

        public async Task<IActionResult> Index()
        {
            var categories = await _categoryApiClient.GetAll();
            return View(categories);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CategoryCreateRequest request)
        {
            if (!ModelState.IsValid)
                return View(request);

            var result = await _categoryApiClient.Create(request);
            if (result != null)
            {
                TempData["SuccessMessage"] = "Thêm mới danh mục thành công";
                return RedirectToAction("Index");
            }

            ModelState.AddModelError("", "Thêm danh mục thất bại");
            return View(request);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            var category = await _categoryApiClient.GetById(id);
            if (category == null)
                return NotFound();

            var updateRequest = new CategoryUpdateRequest()
            {
                Id = category.Id,
                Name = category.Name
            };
            return View(updateRequest);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CategoryUpdateRequest request)
        {
            if (!ModelState.IsValid)
                return View(request);

            var result = await _categoryApiClient.Update(request);
            if (result != null)
            {
                TempData["SuccessMessage"] = "Cập nhật danh mục thành công";
                return RedirectToAction("Index");
            }

            ModelState.AddModelError("", "Cập nhật danh mục thất bại");
            return View(request);
        }
    }
}