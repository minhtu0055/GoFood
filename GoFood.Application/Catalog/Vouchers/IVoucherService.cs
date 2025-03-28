using System;
using System.Threading.Tasks;
using GoFood.Data.Enums;
using GoFood.ViewModels.Catalog.Vouchers;
using GoFood.ViewModels.Common;

namespace GoFood.Application.Catalog.Vouchers
{
    public interface IVoucherService
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