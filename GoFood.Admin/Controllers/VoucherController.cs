using GoFood.ApiIntegration.Services;
using GoFood.Data.Enums;
using GoFood.ViewModels.Catalog.Vouchers;
using GoFood.ViewModels.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;

namespace GoFood.Admin.Controllers
{
    public class VoucherController : BaseController
    {
        private readonly IVoucherApiClient _voucherApiClient;
        private readonly IConfiguration _configuration;

        public VoucherController(IVoucherApiClient voucherApiClient, IConfiguration configuration)
        {
            _voucherApiClient = voucherApiClient;
            _configuration = configuration;
        }

        public async Task<IActionResult> Index(string keyword, PromotionStatus? status, int pageIndex = 1, int pageSize = 10)
        {
            var request = new GetVoucherPagingRequest()
            {
                Keyword = keyword,
                Status = status,
                PageIndex = pageIndex,
                PageSize = pageSize
            };

            var data = await _voucherApiClient.GetAllPaging(request);

            // Tự động cập nhật trạng thái dựa trên thời gian
            var now = DateTime.Now;
            foreach (var item in data.Items)
            {
                var needUpdate = false;
                var newStatus = item.Status;

                // Kiểm tra nếu voucher đã đến thời gian bắt đầu và đang ở trạng thái Inactive
                if (now >= item.StartDate && now <= item.EndDate && item.Status == PromotionStatus.Inactive)
                {
                    newStatus = PromotionStatus.Active;
                    needUpdate = true;
                }
                // Kiểm tra nếu voucher đã hết hạn và chưa ở trạng thái Expired
                else if (now > item.EndDate && item.Status != PromotionStatus.Expired)
                {
                    newStatus = PromotionStatus.Expired;
                    needUpdate = true;
                }

                // Cập nhật trạng thái nếu cần
                if (needUpdate)
                {
                    await _voucherApiClient.UpdateStatus(item.Id, newStatus);
                    item.Status = newStatus;
                }
            }

            ViewBag.Keyword = keyword;
            ViewBag.Status = status;

            ViewBag.StatusSelectList = new List<SelectListItem>()
            {
                new SelectListItem("Tất cả", "", !status.HasValue),
                new SelectListItem("Không hoạt động", PromotionStatus.Inactive.ToString(), status == PromotionStatus.Inactive),
                new SelectListItem("Đang hoạt động", PromotionStatus.Active.ToString(), status == PromotionStatus.Active),
                new SelectListItem("Đã hết hạn", PromotionStatus.Expired.ToString(), status == PromotionStatus.Expired)
            };

            return View(data);
        }

        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.VoucherTypes = new List<SelectListItem>()
            {
                new SelectListItem("Giảm theo phần trăm", DiscountType.Percentage.ToString("d")),
                new SelectListItem("Giảm theo số tiền", DiscountType.Amount.ToString("d"))
            };

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateVoucherRequest request)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.VoucherTypes = new List<SelectListItem>()
                {
                    new SelectListItem("Giảm theo phần trăm", DiscountType.Percentage.ToString("d")),
                    new SelectListItem("Giảm theo số tiền", DiscountType.Amount.ToString("d"))
                };
                return View(request);
            }

            var result = await _voucherApiClient.Create(request);
            if (result != null)
            {
                TempData["SuccessMsg"] = "Tạo mới voucher thành công";
                return RedirectToAction("Index");
            }

            ModelState.AddModelError("", "Tạo mới thất bại");
            ViewBag.VoucherTypes = new List<SelectListItem>()
            {
                new SelectListItem("Giảm theo phần trăm", DiscountType.Percentage.ToString("d")),
                new SelectListItem("Giảm theo số tiền", DiscountType.Amount.ToString("d"))
            };
            return View(request);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            var voucher = await _voucherApiClient.GetById(id);
            if (voucher == null)
                return NotFound();

            var updateRequest = new UpdateVoucherRequest()
            {
                Id = voucher.Id,
                Name = voucher.Name,
                Description = voucher.Description,
                StartDate = voucher.StartDate,
                EndDate = voucher.EndDate,
                DiscountType = voucher.DiscountType,
                DiscountValue = voucher.DiscountValue,
                UsageLimit = voucher.UsageLimit,
                MinimumOrderValue = voucher.MinimumOrderValue
            };

            ViewBag.VoucherTypes = new List<SelectListItem>()
            {
                new SelectListItem("Giảm theo phần trăm", DiscountType.Percentage.ToString("d"), voucher.DiscountType == DiscountType.Percentage),
                new SelectListItem("Giảm theo số tiền", DiscountType.Amount.ToString("d"), voucher.DiscountType == DiscountType.Amount)
            };

            return View(updateRequest);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(UpdateVoucherRequest request)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.VoucherTypes = new List<SelectListItem>()
                {
                    new SelectListItem("Giảm theo phần trăm", DiscountType.Percentage.ToString("d"), request.DiscountType == DiscountType.Percentage),
                    new SelectListItem("Giảm theo số tiền", DiscountType.Amount.ToString("d"), request.DiscountType == DiscountType.Amount)
                };
                return View(request);
            }

            var result = await _voucherApiClient.Update(request);
            if (result != null)
            {
                TempData["SuccessMsg"] = "Cập nhật voucher thành công";
                return RedirectToAction("Index");
            }

            ModelState.AddModelError("", "Cập nhật thất bại");
            ViewBag.VoucherTypes = new List<SelectListItem>()
            {
                new SelectListItem("Giảm theo phần trăm", DiscountType.Percentage.ToString("d"), request.DiscountType == DiscountType.Percentage),
                new SelectListItem("Giảm theo số tiền", DiscountType.Amount.ToString("d"), request.DiscountType == DiscountType.Amount)
            };
            return View(request);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateStatus(Guid id, PromotionStatus status)
        {
            var result = await _voucherApiClient.UpdateStatus(id, status);
            if (result != null)
            {
                TempData["SuccessMsg"] = "Cập nhật trạng thái thành công";
                return RedirectToAction("Index");
            }

            TempData["ErrorMsg"] = "Cập nhật trạng thái thất bại";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Details(Guid id)
        {
            var voucher = await _voucherApiClient.GetById(id);
            if (voucher == null)
                return NotFound();

            return View(voucher);
        }
    }
} 