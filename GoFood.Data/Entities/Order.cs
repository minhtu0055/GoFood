using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GoFood.Data.Enums;

namespace GoFood.Data.Entities
{
    public class Order
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
        public DateTime CreateAt { get; set; } = DateTime.Now;
        public DateTime UpdateAt { get; set; } 
        public Guid UserId { get; set; }
        public AppUsers AppUsers { get; set; }
        public List<OrderDetails> OrderDetails { get; set; }
        
        // Voucher relationship
        public Guid? VoucherId { get; set; }
        public Voucher Voucher { get; set; }
        public decimal DiscountAmount { get; set; }
    }
}
