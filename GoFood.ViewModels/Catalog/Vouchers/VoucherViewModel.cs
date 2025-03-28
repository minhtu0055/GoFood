using System;
using GoFood.Data.Enums;

namespace GoFood.ViewModels.Catalog.Vouchers
{
    public class VoucherViewModel
    {
        public Guid Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DiscountType DiscountType { get; set; }
        public decimal DiscountValue { get; set; }
        public decimal? MinimumOrderValue { get; set; }
        public decimal? MaximumDiscountValue { get; set; }
        public int? UsageLimit { get; set; }
        public int UsageCount { get; set; }
        public PromotionStatus Status { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
    }

    public class CreateVoucherRequest
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DiscountType DiscountType { get; set; }
        public decimal DiscountValue { get; set; }
        public decimal? MinimumOrderValue { get; set; }
        public decimal? MaximumDiscountValue { get; set; }
        public int? UsageLimit { get; set; }
    }

    public class UpdateVoucherRequest
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DiscountType DiscountType { get; set; }
        public decimal DiscountValue { get; set; }
        public decimal? MinimumOrderValue { get; set; }
        public decimal? MaximumDiscountValue { get; set; }
        public int? UsageLimit { get; set; }
    }

    public class GetVoucherPagingRequest
    {
        public string? Keyword { get; set; }
        public PromotionStatus? Status { get; set; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
    }
} 