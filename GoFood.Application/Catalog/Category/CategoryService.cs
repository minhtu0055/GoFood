using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GoFood.Data.EF;
using GoFood.Data.Entities;
using GoFood.ViewModels.Catalog.Category;
using Microsoft.EntityFrameworkCore;

namespace GoFood.Application.Catalog.Category
{
    public class CategoryService : ICategoryService
    {
        private readonly GoFoodDbContext _context;
        public CategoryService(GoFoodDbContext context)
        {
            _context = context;
        }
        public async Task<CategoryViewModels> Create(CategoryCreateRequest request)
        {
            var newCategory = new Data.Entities.Category()
            {
                Id = Guid.NewGuid(),
                Name = request.Name
            };

            _context.Category.Add(newCategory);
            await _context.SaveChangesAsync();

            return new CategoryViewModels()
            {
                Id = newCategory.Id,
                Name = newCategory.Name,
                ProductCount = 0
            };
        }

        public async Task<bool> Delete(Guid id)
        {
            var category = await _context.Category.FindAsync(id);
            if (category == null)
                return false;

            // Kiểm tra xem danh mục có sản phẩm không
            var hasProducts = await _context.Products.AnyAsync(p => p.CategoryId == id);
            if (hasProducts)
                throw new Exception("Không thể xóa danh mục đã có sản phẩm");

            _context.Category.Remove(category);
            var result = await _context.SaveChangesAsync();
            return result > 0;
        }

        public async Task<List<CategoryViewModels>> GetAll()
        {
            var categories = await _context.Category
                .Select(x => new CategoryViewModels()
                {
                    Id = x.Id,
                    Name = x.Name,
                    ProductCount = _context.Products.Count(p => p.CategoryId == x.Id && p.Status == Data.Enums.ProductStatus.Active)
                }).ToListAsync();
            return categories;
        }

        public async Task<CategoryViewModels> GetById(Guid id)
        {
            var category = await _context.Category.FindAsync(id);
            if (category == null)
                throw new Exception($"Không tìm thấy danh mục có id: {id}");

            return new CategoryViewModels()
            {
                Id = category.Id,
                Name = category.Name,
                ProductCount = await _context.Products.CountAsync(p => p.CategoryId == id && p.Status == Data.Enums.ProductStatus.Active)
            };
        }

        public async Task<CategoryViewModels> Update(CategoryUpdateRequest request)
        {
            var category = await _context.Category.FindAsync(request.Id);
            if (category == null)
                throw new Exception($"Không tìm thấy danh mục có id: {request.Id}");

            category.Name = request.Name;
            await _context.SaveChangesAsync();

            return new CategoryViewModels()
            {
                Id = category.Id,
                Name = category.Name,
                ProductCount = await _context.Products.CountAsync(p => p.CategoryId == request.Id && p.Status == Data.Enums.ProductStatus.Active)
            };
        }
    }
}
