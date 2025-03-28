using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GoFood.Data.Enums;
using GoFood.ViewModels.Catalog.Orders;
using GoFood.ViewModels.Common;

namespace GoFood.Application.Catalog.Orders
{
    public interface IOrderService
    {
        Task<PagedResult<OrderViewModel>> GetAllPaging(GetOrderPagingRequest request);
        Task<OrderViewModel> GetById(Guid id);
        Task<List<OrderViewModel>> GetOrdersByUserId(Guid userId);
        Task<bool> UpdateStatus(OrderStatusUpdateRequest request);
        Task<bool> CancelOrder(Guid id);
        Task<OrderViewModel> ExportOrderInvoice(Guid id);
    }
} 