using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GoFood.Application.Common;
using GoFood.Data.EF;
using GoFood.Data.Entities;
using GoFood.ViewModels.Catalog.Combo;
using GoFood.ViewModels.Common;
using Microsoft.EntityFrameworkCore;
using System.IO;

namespace GoFood.Application.Catalog.Combo
{
    public class ComboService : IComboService
    {
        private readonly GoFoodDbContext _context;
        private readonly IStorageService _storageService;

        public ComboService(GoFoodDbContext context, IStorageService storageService)
        {
            _context = context;
            _storageService = storageService;
        }

        public async Task<ComboViewModel> GetById(Guid id)
        {
            var combo = await _context.Combo
                .Include(c => c.ComboProducts)
                    .ThenInclude(cp => cp.Products)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (combo == null)
                return null;

            var viewModel = new ComboViewModel
            {
                Id = combo.Id,
                Name = combo.Name,
                Description = combo.Description,
                Price = combo.Price,
                CreatedDate = combo.CreatedDate,
                ImagePath = combo.ImagePath,
                IsAvailable = combo.IsAvailable,
                ComboProducts = combo.ComboProducts.Select(cp => new ComboProductViewModel
                {
                    ProductId = cp.ProductId,
                    ProductName = cp.Products.Name,
                    ProductPrice = cp.Products.Price,
                    Quantity = cp.Quantity
                }).ToList()
            };

            return viewModel;
        }

        public async Task<PagedResult<ComboViewModel>> GetAllPaging(GetComboPagingRequest request)
        {
            var query = _context.Combo.AsQueryable();

            if (!string.IsNullOrEmpty(request.Keyword))
            {
                query = query.Where(c => c.Name.Contains(request.Keyword) || c.Description.Contains(request.Keyword));
            }

            // Lọc theo giá
            if (request.MinPrice.HasValue)
            {
                query = query.Where(c => c.Price >= request.MinPrice.Value);
            }

            if (request.MaxPrice.HasValue)
            {
                query = query.Where(c => c.Price <= request.MaxPrice.Value);
            }

            // Sắp xếp
            if (!string.IsNullOrEmpty(request.SortBy))
            {
                switch (request.SortBy.ToLower())
                {
                    case "price_asc":
                        query = query.OrderBy(c => c.Price);
                        break;
                    case "price_desc":
                        query = query.OrderByDescending(c => c.Price);
                        break;
                    case "name_asc":
                        query = query.OrderBy(c => c.Name);
                        break;
                    case "name_desc":
                        query = query.OrderByDescending(c => c.Name);
                        break;
                    default:
                        query = query.OrderByDescending(c => c.CreatedDate);
                        break;
                }
            }
            else
            {
                query = query.OrderByDescending(c => c.CreatedDate);
            }

            int totalRow = await query.CountAsync();

            var data = await query
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(c => new ComboViewModel
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    Price = c.Price,
                    CreatedDate = c.CreatedDate,
                    ImagePath = c.ImagePath,
                    IsAvailable = c.IsAvailable,
                    ProductCount = c.ComboProducts.Count
                }).ToListAsync();

            return new PagedResult<ComboViewModel>
            {
                TotalRecords = totalRow,
                PageIndex = request.PageIndex,
                PageSize = request.PageSize,
                Items = data ?? new List<ComboViewModel>()
            };
        }

        public async Task<List<ComboViewModel>> GetAll()
        {
            var combos = await _context.Combo
                .OrderByDescending(c => c.CreatedDate)
                .Select(c => new ComboViewModel
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    Price = c.Price,
                    CreatedDate = c.CreatedDate,
                    ImagePath = c.ImagePath,
                    IsAvailable = c.IsAvailable
                }).ToListAsync();

            return combos;
        }

        public async Task<ComboViewModel> Create(ComboCreateRequest request)
        {
            // Validate input
            if (request.ProductIds.Count != request.Quantities.Count)
            {
                throw new Exception("Số lượng sản phẩm không khớp với số lượng số lượng");
            }

            var combo = new GoFood.Data.Entities.Combo()
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Description = request.Description,
                Price = request.Price,
                CreatedDate = DateTime.Now,
                IsAvailable = request.IsAvailable
            };

            // Xử lý hình ảnh nếu có
            if (request.Image != null)
            {
                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(request.Image.FileName)}";
                await _storageService.SaveFileAsync(request.Image.OpenReadStream(), fileName);
                combo.ImagePath = _storageService.GetFileUrl(fileName);
            }

            _context.Combo.Add(combo);
            await _context.SaveChangesAsync();

            // Thêm sản phẩm vào combo
            for (int i = 0; i < request.ProductIds.Count; i++)
            {
                var productId = request.ProductIds[i];
                var quantity = request.Quantities[i];

                var product = await _context.Products.FindAsync(productId);
                if (product == null)
                    throw new Exception($"Không tìm thấy sản phẩm có id: {productId}");

                if (quantity <= 0)
                    throw new Exception($"Số lượng sản phẩm phải lớn hơn 0");

                var comboProduct = new ComboProduct()
                {
                    ComboId = combo.Id,
                    ProductId = productId,
                    Quantity = quantity
                };

                _context.ComboProducts.Add(comboProduct);
            }

            await _context.SaveChangesAsync();

            return await GetById(combo.Id);
        }

        public async Task<ComboViewModel> Update(ComboUpdateRequest request)
        {
            // Validate input
            if (request.ProductIds.Count != request.Quantities.Count)
            {
                throw new Exception("Số lượng sản phẩm không khớp với số lượng số lượng");
            }

            var combo = await _context.Combo.FindAsync(request.Id);
            if (combo == null)
                throw new Exception($"Không tìm thấy combo có id: {request.Id}");

            combo.Name = request.Name;
            combo.Description = request.Description;
            combo.Price = request.Price;
            combo.IsAvailable = request.IsAvailable;

            // Xử lý hình ảnh nếu có
            if (request.Image != null)
            {
                // Xóa hình ảnh cũ nếu có
                if (!string.IsNullOrEmpty(combo.ImagePath))
                {
                    var oldFileName = Path.GetFileName(combo.ImagePath);
                    await _storageService.DeleteFileAsync(oldFileName);
                }
                
                // Lưu hình ảnh mới
                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(request.Image.FileName)}";
                await _storageService.SaveFileAsync(request.Image.OpenReadStream(), fileName);
                combo.ImagePath = _storageService.GetFileUrl(fileName);
            }

            // Xóa tất cả các combo product cũ
            var existingComboProducts = await _context.ComboProducts
                .Where(cp => cp.ComboId == request.Id)
                .ToListAsync();

            _context.ComboProducts.RemoveRange(existingComboProducts);

            // Thêm các combo product mới
            for (int i = 0; i < request.ProductIds.Count; i++)
            {
                var productId = request.ProductIds[i];
                var quantity = request.Quantities[i];

                var product = await _context.Products.FindAsync(productId);
                if (product == null)
                    throw new Exception($"Không tìm thấy sản phẩm có id: {productId}");

                if (quantity <= 0)
                    throw new Exception($"Số lượng sản phẩm phải lớn hơn 0");

                var comboProduct = new ComboProduct()
                {
                    ComboId = combo.Id,
                    ProductId = productId,
                    Quantity = quantity
                };

                _context.ComboProducts.Add(comboProduct);
            }

            await _context.SaveChangesAsync();

            return await GetById(combo.Id);
        }

        public async Task<bool> Delete(Guid id)
        {
            var combo = await _context.Combo.FindAsync(id);
            if (combo == null)
                return false;

            // Xóa hình ảnh nếu có
            if (!string.IsNullOrEmpty(combo.ImagePath))
            {
                var fileName = Path.GetFileName(combo.ImagePath);
                await _storageService.DeleteFileAsync(fileName);
            }

            // Xóa các combo product liên quan
            var comboProducts = await _context.ComboProducts
                .Where(cp => cp.ComboId == id)
                .ToListAsync();

            foreach (var item in comboProducts)
            {
                _context.ComboProducts.Remove(item);
            }

            _context.Combo.Remove(combo);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateAvailability(Guid id, bool isAvailable)
        {
            var combo = await _context.Combo.FindAsync(id);
            if (combo == null)
                return false;

            combo.IsAvailable = isAvailable;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}