using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GoFood.Application.Common;
using GoFood.Data.EF;
using GoFood.Data.Entities;
using GoFood.Data.Enums;
using GoFood.ViewModels.Catalog.Products;
using GoFood.ViewModels.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace GoFood.Application.Catalog.Products
{
    public class ProductService : IProductService
    {
        private readonly GoFoodDbContext _context;
        private readonly IStorageService _storageService;
        private readonly IConfiguration _configuration;
        public ProductService(GoFoodDbContext context, IStorageService storageService, IConfiguration configuration)
        {
            _context = context;
            _storageService = storageService;
            _configuration = configuration;
        }

        public async Task<ProductViewModels> Create(ProductCreateRequest request)
        {
            var product = new Data.Entities.Products()
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Description = request.Description,
                Price = request.Price,
                Quantity = request.Quantity,
                DateCreated = DateTime.Now,
                IsAvailable = true,
                Status = ProductStatus.Active,
                CategoryId = request.CategoryId
            };

            _context.Products.Add(product);

            if (request.Images?.Count > 0)
            {
                foreach (var image in request.Images)
                {
                    var fileName = await SaveFile(image);
                    var productImage = new ProductImage()
                    {
                        Id = Guid.NewGuid(),
                        ProductId = product.Id,
                        ImageUrl = fileName
                    };
                    _context.ProductImage.Add(productImage);
                }
            }

            await _context.SaveChangesAsync();
            return await GetById(product.Id);
        }

        public async Task<ProductViewModels> Update(ProductUpdateRequest request)
        {
            var product = await _context.Products.FindAsync(request.Id);
            if (product == null) throw new Exception($"Không tìm thấy sản phẩm có id: {request.Id}");

            product.Name = request.Name;
            product.Description = request.Description;
            product.Price = request.Price;
            product.Quantity = request.Quantity;
            product.CategoryId = request.CategoryId;
            product.IsAvailable = request.Quantity > 0;

            await _context.SaveChangesAsync();
            return await GetById(product.Id);
        }

        public async Task<bool> UpdateStatus(Guid id, ProductStatus status)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return false;

            product.Status = status;
            return await _context.SaveChangesAsync() > 0;
        }

        private string GetFullImageUrl(string fileName)
        {
            return _storageService.GetFileUrl(fileName);
        }

        public async Task<ProductViewModels> GetById(Guid id)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.ProductImage)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (product == null) throw new Exception($"Không tìm thấy sản phẩm có id: {id}");

            var result = new ProductViewModels()
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                Quantity = product.Quantity,
                DateCreated = product.DateCreated,
                IsAvailable = product.IsAvailable,
                Status = product.Status,
                CategoryId = product.CategoryId,
                CategoryName = product.Category.Name,
                Images = product.ProductImage.Select(i => i.ImageUrl).ToList()
            };

            // Process image URLs after getting data from database
            result.Images = result.Images.Select(imageUrl => GetFullImageUrl(imageUrl)).ToList();

            return result;
        }

        public async Task<PagedResult<ProductViewModels>> GetAllPaging(GetProductPagingRequest request)
        {
            var query = from p in _context.Products
                        join c in _context.Category on p.CategoryId equals c.Id
                        select new { p, c };

            if (!string.IsNullOrEmpty(request.Keyword))
                query = query.Where(x => x.p.Name.Contains(request.Keyword));

            if (request.CategoryId.HasValue)
                query = query.Where(x => x.p.CategoryId == request.CategoryId);

            if (request.MinPrice.HasValue)
                query = query.Where(x => x.p.Price >= request.MinPrice.Value);

            if (request.MaxPrice.HasValue)
                query = query.Where(x => x.p.Price <= request.MaxPrice.Value);

            if (request.FilterByStatus == true)
                query = query.Where(x => x.p.Status == request.Status);

            int totalRow = await query.CountAsync();

            var data = await query.Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(x => new ProductViewModels()
                {
                    Id = x.p.Id,
                    Name = x.p.Name,
                    Description = x.p.Description,
                    Price = x.p.Price,
                    Quantity = x.p.Quantity,
                    DateCreated = x.p.DateCreated,
                    IsAvailable = x.p.IsAvailable,
                    Status = x.p.Status,
                    CategoryId = x.p.CategoryId,
                    CategoryName = x.c.Name,
                    Images = x.p.ProductImage.Select(i => i.ImageUrl).ToList()
                }).ToListAsync();

            // Process image URLs after getting data from database
            foreach (var item in data)
            {
                item.Images = item.Images.Select(imageUrl => GetFullImageUrl(imageUrl)).ToList();
            }

            var pagedResult = new PagedResult<ProductViewModels>()
            {
                TotalRecords = totalRow,
                PageSize = request.PageSize,
                PageIndex = request.PageIndex,
                Items = data
            };
            return pagedResult;
        }

        public async Task<bool> UpdatePrice(Guid id, decimal newPrice)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return false;

            product.Price = newPrice;
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateStock(Guid id, int addedQuantity)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return false;

            product.Quantity += addedQuantity;
            product.IsAvailable = product.Quantity > 0;

            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<int> AddImage(Guid productId, IFormFile image)
        {
            var fileName = await SaveFile(image);
            var productImage = new ProductImage()
            {
                Id = Guid.NewGuid(),
                ProductId = productId,
                ImageUrl = fileName
            };

            _context.ProductImage.Add(productImage);
            return await _context.SaveChangesAsync();
        }

        public async Task<int> RemoveImage(Guid imageId)
        {
            try
            {
                Console.WriteLine($"[ProductService] RemoveImage called with imageId: {imageId}");
                
                var image = await _context.ProductImage.FindAsync(imageId);
                if (image == null) 
                {
                    Console.WriteLine($"[ProductService] Image with ID {imageId} not found");
                    return 0;
                }

                Console.WriteLine($"[ProductService] Found image with ID {imageId}, ProductId: {image.ProductId}, URL: {image.ImageUrl}");
                
                try 
                {
                    await _storageService.DeleteFileAsync(image.ImageUrl);
                    Console.WriteLine($"[ProductService] DeleteFileAsync completed for {image.ImageUrl}");
                }
                catch (Exception fileEx)
                {
                    Console.WriteLine($"[ProductService] Error deleting file {image.ImageUrl}: {fileEx.Message}");
                    // Tiếp tục xử lý mặc dù có lỗi khi xóa file vật lý
                }
                
                _context.ProductImage.Remove(image);
                var result = await _context.SaveChangesAsync();
                Console.WriteLine($"[ProductService] Database update completed with result: {result}");
                
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ProductService] Exception in RemoveImage: {ex.Message}");
                Console.WriteLine($"[ProductService] Stack Trace: {ex.StackTrace}");
                throw; // Re-throw to let caller handle
            }
        }

        public async Task<List<string>> GetListImages(Guid productId)
        {
            return await _context.ProductImage
                .Where(x => x.ProductId == productId)
                .Select(i => i.ImageUrl)
                .ToListAsync();
        }

        private async Task<string> SaveFile(IFormFile file)
        {
            var originalFileName = file.FileName;
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(originalFileName)}";
            await _storageService.SaveFileAsync(file.OpenReadStream(), fileName);
            return fileName;
        }

        public async Task<List<ProductViewModels>> GetAll()
        {
            var products = await _context.Products
                .Include(x => x.Category)
                .Include(x => x.ProductImage)
                .Select(x => new ProductViewModels()
                {
                    Id = x.Id,
                    Name = x.Name,
                    Description = x.Description,
                    Price = x.Price,
                    Quantity = x.Quantity,
                    DateCreated = x.DateCreated,
                    IsAvailable = x.IsAvailable,
                    Status = x.Status,
                    CategoryId = x.CategoryId,
                    CategoryName = x.Category.Name,
                    Images = x.ProductImage.Select(i => i.ImageUrl).ToList()
                }).ToListAsync();

            // Process image URLs after getting data from database
            foreach (var product in products)
            {
                product.Images = product.Images.Select(imageUrl => GetFullImageUrl(imageUrl)).ToList();
            }

            return products;
        }
    }
}
