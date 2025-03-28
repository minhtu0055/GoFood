using System;
using System.Collections.Generic;
using GoFood.Data.Enums;

namespace GoFood.Data.Entities
{
    public class Promotion
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

        public List<PromotionProduct> PromotionProducts { get; set; }
    }
} 