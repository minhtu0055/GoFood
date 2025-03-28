using System;
using System.Linq;
using System.Threading.Tasks;
using GoFood.Data.EF;
using GoFood.Data.Entities;
using GoFood.Data.Enums;
using GoFood.ViewModels.Catalog.Vouchers;
using GoFood.ViewModels.Common;
using Microsoft.EntityFrameworkCore;

namespace GoFood.Application.Catalog.Vouchers
{
    public class VoucherService : IVoucherService
    {
        private readonly GoFoodDbContext _context;

        public VoucherService(GoFoodDbContext context)
        {
            _context = context;
        }

        public async Task<VoucherViewModel> Create(CreateVoucherRequest request)
        {
            var voucher = new Voucher()
            {
                Id = Guid.NewGuid(),
                Code = request.Code,
                Name = request.Name,
                Description = request.Description,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                DiscountType = request.DiscountType,
                DiscountValue = request.DiscountValue,
                MinimumOrderValue = request.MinimumOrderValue,
                MaximumDiscountValue = request.MaximumDiscountValue,
                UsageLimit = request.UsageLimit,
                UsageCount = 0,
                Status = DateTime.Now >= request.StartDate ? PromotionStatus.Active : PromotionStatus.Inactive,
                CreatedDate = DateTime.Now
            };

            _context.Vouchers.Add(voucher);
            await _context.SaveChangesAsync();

            return await GetById(voucher.Id);
        }

        public async Task<VoucherViewModel> Update(UpdateVoucherRequest request)
        {
            var voucher = await _context.Vouchers.FindAsync(request.Id);
            if (voucher == null) throw new Exception($"Không tìm thấy voucher có id: {request.Id}");

            voucher.Name = request.Name;
            voucher.Description = request.Description;
            voucher.StartDate = request.StartDate;
            voucher.EndDate = request.EndDate;
            voucher.DiscountType = request.DiscountType;
            voucher.DiscountValue = request.DiscountValue;
            voucher.MinimumOrderValue = request.MinimumOrderValue;
            voucher.MaximumDiscountValue = request.MaximumDiscountValue;
            voucher.UsageLimit = request.UsageLimit;
            voucher.ModifiedDate = DateTime.Now;

            // Cập nhật trạng thái dựa trên ngày và số lần sử dụng
            await UpdateVoucherStatus(voucher);

            await _context.SaveChangesAsync();
            return await GetById(voucher.Id);
        }

        public async Task<bool> UpdateStatus(Guid id, PromotionStatus status)
        {
            var voucher = await _context.Vouchers.FindAsync(id);
            if (voucher == null) return false;

            voucher.Status = status;
            voucher.ModifiedDate = DateTime.Now;

            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<VoucherViewModel> GetById(Guid id)
        {
            var voucher = await _context.Vouchers
                .FirstOrDefaultAsync(x => x.Id == id);

            if (voucher == null) throw new Exception($"Không tìm thấy voucher có id: {id}");

            return new VoucherViewModel
            {
                Id = voucher.Id,
                Code = voucher.Code,
                Name = voucher.Name,
                Description = voucher.Description,
                StartDate = voucher.StartDate,
                EndDate = voucher.EndDate,
                DiscountType = voucher.DiscountType,
                DiscountValue = voucher.DiscountValue,
                MinimumOrderValue = voucher.MinimumOrderValue,
                MaximumDiscountValue = voucher.MaximumDiscountValue,
                UsageLimit = voucher.UsageLimit,
                UsageCount = voucher.UsageCount,
                Status = voucher.Status,
                CreatedDate = voucher.CreatedDate,
                ModifiedDate = voucher.ModifiedDate
            };
        }

        public async Task<VoucherViewModel> GetByCode(string code)
        {
            var voucher = await _context.Vouchers
                .FirstOrDefaultAsync(x => x.Code == code);

            if (voucher == null) throw new Exception($"Không tìm thấy voucher có mã: {code}");

            return new VoucherViewModel
            {
                Id = voucher.Id,
                Code = voucher.Code,
                Name = voucher.Name,
                Description = voucher.Description,
                StartDate = voucher.StartDate,
                EndDate = voucher.EndDate,
                DiscountType = voucher.DiscountType,
                DiscountValue = voucher.DiscountValue,
                MinimumOrderValue = voucher.MinimumOrderValue,
                MaximumDiscountValue = voucher.MaximumDiscountValue,
                UsageLimit = voucher.UsageLimit,
                UsageCount = voucher.UsageCount,
                Status = voucher.Status,
                CreatedDate = voucher.CreatedDate,
                ModifiedDate = voucher.ModifiedDate
            };
        }

        public async Task<PagedResult<VoucherViewModel>> GetAllPaging(GetVoucherPagingRequest request)
        {
            var query = _context.Vouchers.AsQueryable();

            if (!string.IsNullOrEmpty(request.Keyword))
            {
                query = query.Where(x => x.Name.Contains(request.Keyword) 
                    || x.Code.Contains(request.Keyword));
            }

            if (request.Status.HasValue)
            {
                query = query.Where(x => x.Status == request.Status.Value);
            }

            int totalRow = await query.CountAsync();

            var data = await query.Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(x => new VoucherViewModel()
                {
                    Id = x.Id,
                    Code = x.Code,
                    Name = x.Name,
                    Description = x.Description,
                    StartDate = x.StartDate,
                    EndDate = x.EndDate,
                    DiscountType = x.DiscountType,
                    DiscountValue = x.DiscountValue,
                    MinimumOrderValue = x.MinimumOrderValue,
                    MaximumDiscountValue = x.MaximumDiscountValue,
                    UsageLimit = x.UsageLimit,
                    UsageCount = x.UsageCount,
                    Status = x.Status,
                    CreatedDate = x.CreatedDate,
                    ModifiedDate = x.ModifiedDate
                }).ToListAsync();

            var pagedResult = new PagedResult<VoucherViewModel>()
            {
                TotalRecords = totalRow,
                PageSize = request.PageSize,
                PageIndex = request.PageIndex,
                Items = data
            };
            return pagedResult;
        }

        public async Task<bool> UseVoucher(string code)
        {
            var voucher = await _context.Vouchers
                .FirstOrDefaultAsync(x => x.Code == code);
            if (voucher == null) return false;

            // Kiểm tra trạng thái và giới hạn sử dụng
            if (voucher.Status != PromotionStatus.Active) return false;
            if (voucher.UsageLimit.HasValue && voucher.UsageCount >= voucher.UsageLimit.Value) return false;

            voucher.UsageCount++;
            voucher.ModifiedDate = DateTime.Now;

            // Cập nhật trạng thái nếu cần
            await UpdateVoucherStatus(voucher);

            return await _context.SaveChangesAsync() > 0;
        }

        private async Task UpdateVoucherStatus(Voucher voucher)
        {
            var now = DateTime.Now;
            
            if (voucher.UsageLimit.HasValue && voucher.UsageCount >= voucher.UsageLimit.Value)
            {
                voucher.Status = PromotionStatus.Expired;
            }
            else if (now >= voucher.StartDate && now <= voucher.EndDate)
            {
                voucher.Status = PromotionStatus.Active;
            }
            else if (now < voucher.StartDate)
            {
                voucher.Status = PromotionStatus.Inactive;
            }
            else
            {
                voucher.Status = PromotionStatus.Expired;
            }
        }
    }
} 