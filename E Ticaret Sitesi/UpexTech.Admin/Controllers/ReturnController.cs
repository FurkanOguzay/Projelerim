using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UpexTech.Business.Services;
using UpexTech.Entity;

namespace UpexTech.Admin.Controllers
{
    [Authorize]
    public class ReturnController : AdminBaseController
    {
        private readonly IReturnService _returnService;

        public ReturnController(IReturnService returnService)
        {
            _returnService = returnService;
        }

        public async Task<IActionResult> Index(ReturnStatus? status, string? search, int page = 1)
        {
            var pageSize = 20;
            var (returns, totalCount) = await _returnService.GetReturnsPagedAsync(page, pageSize, status, search);
            var statusCounts = await _returnService.GetReturnCountByStatusAsync();

            ViewBag.StatusCounts = statusCounts;
            ViewBag.TotalCount = totalCount;
            ViewBag.CurrentStatus = status;
            ViewBag.CurrentSearch = search;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            return View(returns);
        }

        public async Task<IActionResult> Details(int id)
        {
            var returnRequest = await _returnService.GetReturnByIdAsync(id);
            if (returnRequest == null)
            {
                return NotFound();
            }
            return View(returnRequest);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, ReturnStatus status, string? adminNotes)
        {
            var result = await _returnService.UpdateReturnStatusAsync(id, status, adminNotes);
            if (!result)
            {
                return Json(new { success = false, message = "İade talebi bulunamadı." });
            }

            return Json(new { success = true, message = "İade durumu güncellendi." });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            var result = await _returnService.UpdateReturnStatusAsync(id, ReturnStatus.Approved);
            return Json(new { success = result, message = result ? "İade onaylandı." : "Hata oluştu." });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id, string? reason)
        {
            var result = await _returnService.UpdateReturnStatusAsync(id, ReturnStatus.Rejected, reason);
            return Json(new { success = result, message = result ? "İade reddedildi." : "Hata oluştu." });
        }

        private string GetStatusDisplayText(ReturnStatus status)
        {
            return status switch
            {
                ReturnStatus.Pending => "Onay Bekliyor",
                ReturnStatus.Approved => "Onaylandı",
                ReturnStatus.Rejected => "Reddedildi",
                ReturnStatus.InTransit => "Kargoda",
                ReturnStatus.Received => "Teslim Alındı",
                ReturnStatus.Refunded => "İade Edildi",
                ReturnStatus.Disputed => "İhtilaflı",
                _ => status.ToString()
            };
        }
    }
}
