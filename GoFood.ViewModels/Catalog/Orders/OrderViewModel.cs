using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GoFood.Data.Enums;

namespace GoFood.ViewModels.Catalog.Orders
{
    public class OrderViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public Status Status { get; set; }
        public string PaymentMethod { get; set; }
        public string ShippingAddress { get; set; }
        public string PhoneNumber { get; set; }
        public string Notes { get; set; }
        public DateTime CreateAt { get; set; }
        public DateTime UpdateAt { get; set; }
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        
        // Voucher info
        public Guid? VoucherId { get; set; }
        public string VoucherCode { get; set; }
        public decimal DiscountAmount { get; set; }
        
        // Order details
        public List<OrderDetailViewModel> OrderDetails { get; set; }
    }
    
    public class OrderDetailViewModel
    {
        public Guid Id { get; set; }
        public Guid OrderId { get; set; }
        public Guid ProductId { get; set; }
        public string ProductName { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public decimal Total { get; set; }
        public string ProductImage { get; set; }
        public string ComboImage { get; set; }
        public bool IsCombo { get; set; }
    }
} 