using GoFood.Data.Enums;
using GoFood.ViewModels.Catalog.Vouchers;
using GoFood.ViewModels.Common;

namespace GoFood.ApiIntegration.Services
{
    public interface IVoucherApiClient
    {
        Task<VoucherViewModel> Create(CreateVoucherRequest request);
        Task<VoucherViewModel> Update(UpdateVoucherRequest request);
        Task<bool> UpdateStatus(Guid id, PromotionStatus status);
        Task<VoucherViewModel> GetById(Guid id);
        Task<VoucherViewModel> GetByCode(string code);
        Task<PagedResult<VoucherViewModel>> GetAllPaging(GetVoucherPagingRequest request);
        Task<bool> UseVoucher(string code);
    }
}
