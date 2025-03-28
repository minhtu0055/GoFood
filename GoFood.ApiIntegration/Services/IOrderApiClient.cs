using GoFood.ViewModels.Catalog.Orders;
using GoFood.ViewModels.Common;

namespace GoFood.ApiIntegration.Services
{
    public interface IOrderApiClient
    {
        Task<PagedResult<OrderViewModel>> GetAllPaging(GetOrderPagingRequest request);
        Task<OrderViewModel> GetById(Guid id);
        Task<List<OrderViewModel>> GetOrdersByUserId(Guid userId);
        Task<bool> UpdateStatus(Guid id, OrderStatusUpdateRequest request);
        Task<bool> CancelOrder(Guid id);
        Task<OrderViewModel> ExportInvoice(Guid id);
    }
} 