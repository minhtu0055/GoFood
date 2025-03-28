using System;
using System.Collections.Generic;
using GoFood.Data.Enums;

namespace GoFood.ViewModels.Catalog.Promotions
{
    public class PromotionViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DiscountType DiscountType { get; set; }
        public decimal DiscountValue { get; set; }
        public PromotionStatus Status { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public List<PromotionProductViewModel> Products { get; set; }
    }

    public class PromotionProductViewModel
    {
        public Guid ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductImage { get; set; }
        public decimal OriginalPrice { get; set; }
        public decimal PromotionalPrice { get; set; }
    }

    public class CreatePromotionRequest
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DiscountType DiscountType { get; set; }
        public decimal DiscountValue { get; set; }
        public List<Guid> ProductIds { get; set; }
    }

    public class UpdatePromotionRequest
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DiscountType DiscountType { get; set; }
        public decimal DiscountValue { get; set; }
        public List<Guid> ProductIds { get; set; }
    }

    public class GetPromotionPagingRequest
    {
        public string? Keyword { get; set; }
        public PromotionStatus? Status { get; set; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
    }
} 