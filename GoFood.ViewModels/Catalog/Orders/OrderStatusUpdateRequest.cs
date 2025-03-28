using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GoFood.Data.Enums;

namespace GoFood.ViewModels.Catalog.Orders
{
    public class OrderStatusUpdateRequest
    {
        public Guid Id { get; set; }
        
        [Required(ErrorMessage = "Trạng thái không được để trống")]
        public Status Status { get; set; }
    }
} 