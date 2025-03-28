using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GoFood.Data.EF;
using GoFood.Data.Entities;
using GoFood.Data.Enums;
using GoFood.ViewModels.Catalog.Promotions;
using GoFood.ViewModels.Common;
using Microsoft.EntityFrameworkCore;

namespace GoFood.Application.Catalog.Promotions
{
    public class PromotionService : IPromotionService
    {
        private readonly GoFoodDbContext _context;

        public PromotionService(GoFoodDbContext context)
        {
            _context = context;
        }

        public async Task<PromotionViewModel> Create(CreatePromotionRequest request)
        {
            var promotion = new Promotion()
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Description = request.Description,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                DiscountType = request.DiscountType,
                DiscountValue = request.DiscountValue,
                Status = DateTime.Now >= request.StartDate ? PromotionStatus.Active : PromotionStatus.Inactive,
                CreatedDate = DateTime.Now,
                ModifiedDate = DateTime.Now
            };

            foreach (var productId in request.ProductIds)
            {
                var product = await _context.Products.FindAsync(productId);
                if (product == null)
                    throw new Exception($"Không tìm thấy sản phẩm có id: {productId}");

                var promotionProduct = new PromotionProduct()
                {
                    ProductId = productId,
                    PromotionId = promotion.Id
                };
                _context.PromotionProducts.Add(promotionProduct);
            }

            _context.Promotions.Add(promotion);
            await _context.SaveChangesAsync();

            return await GetById(promotion.Id);
        }

        public async Task<PromotionViewModel> Update(UpdatePromotionRequest request)
        {
            var promotion = await _context.Promotions.FindAsync(request.Id);
            if (promotion == null)
                throw new Exception($"Không tìm thấy khuyến mãi có id: {request.Id}");

            promotion.Name = request.Name;
            promotion.Description = request.Description;
            promotion.StartDate = request.StartDate;
            promotion.EndDate = request.EndDate;
            promotion.DiscountType = request.DiscountType;
            promotion.DiscountValue = request.DiscountValue;
            promotion.ModifiedDate = DateTime.Now;

            // Cập nhật trạng thái dựa trên thời gian
            await UpdatePromotionStatus(promotion);

            // Xóa các sản phẩm cũ
            var oldProducts = await _context.PromotionProducts
                .Where(x => x.PromotionId == request.Id)
                .ToListAsync();
            _context.PromotionProducts.RemoveRange(oldProducts);

            // Thêm các sản phẩm mới
            foreach (var productId in request.ProductIds)
            {
                var product = await _context.Products.FindAsync(productId);
                if (product == null)
                    throw new Exception($"Không tìm thấy sản phẩm có id: {productId}");

                var promotionProduct = new PromotionProduct()
                {
                    ProductId = productId,
                    PromotionId = promotion.Id
                };
                _context.PromotionProducts.Add(promotionProduct);
            }

            await _context.SaveChangesAsync();

            return await GetById(request.Id);
        }

        public async Task<bool> UpdateStatus(Guid id, PromotionStatus status)
        {
            var promotion = await _context.Promotions.FindAsync(id);
            if (promotion == null)
                throw new Exception($"Không tìm thấy khuyến mãi có id: {id}");

            promotion.Status = status;
            promotion.ModifiedDate = DateTime.Now;

            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<PromotionViewModel> GetById(Guid id)
        {
            var promotion = await _context.Promotions
                .Include(x => x.PromotionProducts)
                .ThenInclude(x => x.Product)
                .ThenInclude(x => x.ProductImage)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (promotion == null)
                throw new Exception($"Không tìm thấy khuyến mãi có id: {id}");

            return new PromotionViewModel
            {
                Id = promotion.Id,
                Name = promotion.Name,
                Description = promotion.Description,
                StartDate = promotion.StartDate,
                EndDate = promotion.EndDate,
                DiscountType = promotion.DiscountType,
                DiscountValue = promotion.DiscountValue,
                Status = promotion.Status,
                CreatedDate = promotion.CreatedDate,
                ModifiedDate = promotion.ModifiedDate,
                Products = promotion.PromotionProducts.Select(p => new PromotionProductViewModel()
                {
                    ProductId = p.ProductId,
                    ProductName = p.Product.Name,
                    ProductImage = p.Product.ProductImage.Select(i => i.ImageUrl).FirstOrDefault(),
                    OriginalPrice = p.Product.Price,
                    PromotionalPrice = promotion.DiscountType == DiscountType.Percentage 
                        ? p.Product.Price * (100 - promotion.DiscountValue) / 100
                        : p.Product.Price - promotion.DiscountValue
                }).ToList()
            };
        }

        public async Task<PagedResult<PromotionViewModel>> GetAllPaging(GetPromotionPagingRequest request)
        {
            var query = _context.Promotions
                .Include(x => x.PromotionProducts)
                .ThenInclude(x => x.Product)
                .ThenInclude(x => x.ProductImage)
                .AsQueryable();

            // Cập nhật trạng thái cho tất cả các promotion
            var promotions = await query.ToListAsync();
            foreach (var promotion in promotions)
            {
                await UpdatePromotionStatus(promotion);
            }
            await _context.SaveChangesAsync();

            if (!string.IsNullOrEmpty(request.Keyword))
            {
                query = query.Where(x => x.Name.Contains(request.Keyword));
            }

            if (request.Status.HasValue)
            {
                query = query.Where(x => x.Status == request.Status.Value);
            }

            int totalRow = await query.CountAsync();

            var data = await query.Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(x => new PromotionViewModel()
                {
                    Id = x.Id,
                    Name = x.Name,
                    Description = x.Description,
                    StartDate = x.StartDate,
                    EndDate = x.EndDate,
                    DiscountType = x.DiscountType,
                    DiscountValue = x.DiscountValue,
                    Status = x.Status,
                    CreatedDate = x.CreatedDate,
                    ModifiedDate = x.ModifiedDate,
                    Products = x.PromotionProducts.Select(p => new PromotionProductViewModel()
                    {
                        ProductId = p.ProductId,
                        ProductName = p.Product.Name,
                        ProductImage = p.Product.ProductImage.Select(i => i.ImageUrl).FirstOrDefault(),
                        OriginalPrice = p.Product.Price,
                        PromotionalPrice = x.DiscountType == DiscountType.Percentage
                            ? p.Product.Price * (100 - x.DiscountValue) / 100
                            : p.Product.Price - x.DiscountValue
                    }).ToList()
                }).ToListAsync();

            var pagedResult = new PagedResult<PromotionViewModel>()
            {
                TotalRecords = totalRow,
                PageSize = request.PageSize,
                PageIndex = request.PageIndex,
                Items = data
            };
            return pagedResult;
        }

        private async Task UpdatePromotionStatus(Promotion promotion)
        {
            var now = DateTime.Now;
            
            if (now >= promotion.StartDate && now <= promotion.EndDate)
            {
                promotion.Status = PromotionStatus.Active;
            }
            else if (now < promotion.StartDate)
            {
                promotion.Status = PromotionStatus.Inactive;
            }
            else
            {
                promotion.Status = PromotionStatus.Expired;
            }
        }
    }
} 