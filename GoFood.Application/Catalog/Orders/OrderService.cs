using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GoFood.Application.Catalog.Vouchers;
using GoFood.Data.EF;
using GoFood.Data.Entities;
using GoFood.Data.Enums;
using GoFood.ViewModels.Catalog.Orders;
using GoFood.ViewModels.Common;
using Microsoft.EntityFrameworkCore;

namespace GoFood.Application.Catalog.Orders
{
    public class OrderService : IOrderService
    {
        private readonly GoFoodDbContext _context;
        private readonly IVoucherService _voucherService;

        public OrderService(GoFoodDbContext context, IVoucherService voucherService)
        {
            _context = context;
            _voucherService = voucherService;
        }

        public async Task<PagedResult<OrderViewModel>> GetAllPaging(GetOrderPagingRequest request)
        {
            var query = _context.Order.AsQueryable();

            // Lọc theo từ khóa (tìm kiếm theo tên hoặc số điện thoại)
            if (!string.IsNullOrEmpty(request.Keyword))
            {
                query = query.Where(x => x.Name.Contains(request.Keyword) || 
                                        x.PhoneNumber.Contains(request.Keyword));
            }

            // Lọc theo người dùng
            if (request.UserId.HasValue)
            {
                query = query.Where(x => x.UserId == request.UserId.Value);
            }

            // Lọc theo trạng thái
            if (request.Status.HasValue)
            {
                query = query.Where(x => x.Status == request.Status.Value);
            }

            // Lọc theo ngày đặt hàng
            if (request.FromDate.HasValue)
            {
                query = query.Where(x => x.OrderDate >= request.FromDate.Value);
            }

            if (request.ToDate.HasValue)
            {
                var toDateEnd = request.ToDate.Value.AddDays(1).AddSeconds(-1); // Đến cuối ngày đã chọn
                query = query.Where(x => x.OrderDate <= toDateEnd);
            }

            // Đếm tổng số bản ghi thỏa mãn
            int totalRow = await query.CountAsync();

            // Lấy dữ liệu phân trang
            var data = await query
                .Include(x => x.AppUsers)
                .Include(x => x.OrderDetails)
                .ThenInclude(x => x.Products)
                .Include(x => x.Voucher)
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(x => new OrderViewModel()
                {
                    Id = x.Id,
                    Name = x.Name,
                    OrderDate = x.OrderDate,
                    TotalAmount = x.TotalAmount,
                    Status = x.Status,
                    PaymentMethod = x.PaymentMethod,
                    ShippingAddress = x.ShippingAddress,
                    PhoneNumber = x.PhoneNumber,
                    Notes = x.Notes,
                    CreateAt = x.CreateAt,
                    UpdateAt = x.UpdateAt,
                    UserId = x.UserId,
                    UserName = x.AppUsers.UserName,
                    VoucherId = x.VoucherId,
                    VoucherCode = x.Voucher != null ? x.Voucher.Code : null,
                    DiscountAmount = x.DiscountAmount,
                    OrderDetails = x.OrderDetails.Select(od => new OrderDetailViewModel()
                    {
                        Id = od.Id,
                        OrderId = od.OrderId,
                        ProductId = od.ProductId,
                        ProductName = od.Products.Name,
                        Price = od.SellingPrice,
                        Quantity = od.Quantity,
                        Total = od.SellingPrice * od.Quantity
                    }).ToList()
                })
                .ToListAsync();

            // Trả về kết quả phân trang
            var pagedResult = new PagedResult<OrderViewModel>()
            {
                TotalRecords = totalRow,
                PageSize = request.PageSize,
                PageIndex = request.PageIndex,
                Items = data
            };
            return pagedResult;
        }

        public async Task<OrderViewModel> GetById(Guid id)
        {
            try
            {
                var order = await _context.Order
                    .Include(x => x.OrderDetails)
                    .ThenInclude(x => x.Products)
                    .Include(x => x.Voucher)
                    .FirstOrDefaultAsync(x => x.Id == id);

                if (order == null)
                    return null;

                // Lấy thông tin người dùng từ bảng AspNetUsers:
                var userName = "";
                if (order.UserId != Guid.Empty)
                {
                    // Chuyển đổi Guid sang string để tìm kiếm trong bảng AspNetUsers
                    var userIdString = order.UserId.ToString();
                    var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userIdString);
                    userName = user?.UserName ?? "";
                }
                
                return new OrderViewModel()
                {
                    Id = order.Id,
                    Name = order.Name,
                    OrderDate = order.OrderDate,
                    TotalAmount = order.TotalAmount,
                    Status = order.Status,
                    PaymentMethod = order.PaymentMethod,
                    ShippingAddress = order.ShippingAddress,
                    PhoneNumber = order.PhoneNumber,
                    Notes = order.Notes,
                    CreateAt = order.CreateAt,
                    UpdateAt = order.UpdateAt,
                    UserId = order.UserId,
                    UserName = userName,
                    VoucherId = order.VoucherId,
                    VoucherCode = order.Voucher?.Code,
                    DiscountAmount = order.DiscountAmount,
                    OrderDetails = order.OrderDetails.Select(od => new OrderDetailViewModel()
                    {
                        Id = od.Id,
                        OrderId = od.OrderId,
                        ProductId = od.ProductId,
                        ProductName = od.Products?.Name,
                        Price = od.SellingPrice,
                        Quantity = od.Quantity,
                        Total = od.SellingPrice * od.Quantity
                    }).ToList()
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetById: {ex.Message}");
                throw;
            }
        }

        public async Task<List<OrderViewModel>> GetOrdersByUserId(Guid userId)
        {
            var orders = await _context.Order
                .Include(x => x.AppUsers)
                .Include(x => x.OrderDetails)
                .ThenInclude(x => x.Products)
                .Include(x => x.Voucher)
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.OrderDate)
                .Select(x => new OrderViewModel()
                {
                    Id = x.Id,
                    Name = x.Name,
                    OrderDate = x.OrderDate,
                    TotalAmount = x.TotalAmount,
                    Status = x.Status,
                    PaymentMethod = x.PaymentMethod,
                    ShippingAddress = x.ShippingAddress,
                    PhoneNumber = x.PhoneNumber,
                    Notes = x.Notes,
                    CreateAt = x.CreateAt,
                    UpdateAt = x.UpdateAt,
                    UserId = x.UserId,
                    UserName = x.AppUsers.UserName,
                    VoucherId = x.VoucherId,
                    VoucherCode = x.Voucher != null ? x.Voucher.Code : null,
                    DiscountAmount = x.DiscountAmount,
                    OrderDetails = x.OrderDetails.Select(od => new OrderDetailViewModel()
                    {
                        Id = od.Id,
                        OrderId = od.OrderId,
                        ProductId = od.ProductId,
                        ProductName = od.Products.Name,
                        Price = od.SellingPrice,
                        Quantity = od.Quantity,
                        Total = od.SellingPrice * od.Quantity
                    }).ToList()
                })
                .ToListAsync();

            return orders;
        }

        public async Task<bool> UpdateStatus(OrderStatusUpdateRequest request)
        {
            var order = await _context.Order.FindAsync(request.Id);
            if (order == null)
                return false;

            // Không cho phép cập nhật trạng thái nếu đơn hàng đã hủy hoặc đã hoàn thành
            if (order.Status == Status.Cancelled || order.Status == Status.Completed)
                return false;

            order.Status = request.Status;
            order.UpdateAt = DateTime.Now;

            // Nếu trạng thái là hủy hoặc hoàn thành, cập nhật số lượng sản phẩm
            if (request.Status == Status.Cancelled)
            {
                // Trả lại số lượng sản phẩm vào kho
                await ReturnProductsToInventory(order.Id);
                
                // Nếu đơn hàng đã sử dụng voucher, giảm số lần sử dụng của voucher
                if (order.VoucherId.HasValue)
                {
                    var voucher = await _context.Vouchers.FindAsync(order.VoucherId.Value);
                    if (voucher != null && voucher.UsageCount > 0)
                    {
                        voucher.UsageCount -= 1;
                    }
                }
            }

            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> CancelOrder(Guid id)
        {
            var order = await _context.Order.FindAsync(id);
            if (order == null)
                return false;

            // Chỉ có thể hủy đơn hàng khi đơn hàng ở trạng thái chờ xác nhận hoặc đã xác nhận
            if (order.Status != Status.Pending && order.Status != Status.Confirmed)
                return false;

            order.Status = Status.Cancelled;
            order.UpdateAt = DateTime.Now;

            // Trả lại số lượng sản phẩm vào kho
            await ReturnProductsToInventory(order.Id);
            
            // Nếu đơn hàng đã sử dụng voucher, giảm số lần sử dụng của voucher
            if (order.VoucherId.HasValue)
            {
                var voucher = await _context.Vouchers.FindAsync(order.VoucherId.Value);
                if (voucher != null && voucher.UsageCount > 0)
                {
                    voucher.UsageCount -= 1;
                }
            }

            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<OrderViewModel> ExportOrderInvoice(Guid id)
        {
            // Lấy thông tin chi tiết của đơn hàng
            var order = await GetById(id);
            if (order == null)
                return null;

            // Trong thực tế, đây là nơi bạn sẽ tạo PDF hoặc file xuất khẩu khác
            // Tuy nhiên, trong phạm vi này, chúng ta chỉ trả về thông tin đơn hàng
            return order;
        }

        // Phương thức hỗ trợ để trả lại số lượng sản phẩm vào kho khi hủy đơn hàng
        private async Task ReturnProductsToInventory(Guid orderId)
        {
            var orderDetails = await _context.OrderDetails
                .Where(x => x.OrderId == orderId)
                .ToListAsync();

            foreach (var detail in orderDetails)
            {
                var product = await _context.Products.FindAsync(detail.ProductId);
                if (product != null)
                {
                    product.Quantity += detail.Quantity;
                }
            }
        }
    }
}