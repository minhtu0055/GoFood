using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GoFood.Data.Enums;
using GoFood.ViewModels.Common;
using GoFood.ViewModels.System.Users;

namespace GoFood.ViewModels.Catalog.Orders
{
    public class GetOrderPagingRequest : PagingRequestBase
    {
        public string? Keyword { get; set; }
        public Guid? UserId { get; set; }
        public Status? Status { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }
} 