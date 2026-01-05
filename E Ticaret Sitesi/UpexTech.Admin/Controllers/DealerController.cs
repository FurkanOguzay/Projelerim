using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UpexTech.Business.Services;
using UpexTech.Entity;

namespace UpexTech.Admin.Controllers
{
    [Authorize]
    public class DealerController : AdminBaseController
    {
        private readonly IUserService _userService;

        public DealerController(IUserService userService)
        {
            _userService = userService;
        }

        public async Task<IActionResult> Index()
        {
            var pendingDealers = await _userService.GetPendingDealersAsync();
            return View(pendingDealers);
        }

        public async Task<IActionResult> Details(int id)
        {
            var dealer = await _userService.GetByIdAsync(id);
            if (dealer == null || dealer.Role != UserRole.B2B)
            {
                return NotFound();
            }
            return View(dealer);
        }

        [HttpPost]
        public async Task<IActionResult> Approve(int id)
        {
            await _userService.ApproveDealerAsync(id);
            TempData["SuccessMessage"] = "Bayi başvurusu onaylandı.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Reject(int id)
        {
            await _userService.RejectDealerAsync(id);
            TempData["SuccessMessage"] = "Bayi başvurusu reddedildi.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> SetCreditLimit(int id, decimal creditLimit)
        {
            await _userService.UpdateCreditLimitAsync(id, creditLimit);
            TempData["SuccessMessage"] = "Kredi limiti tanımlandı.";
            return RedirectToAction("Details", new { id });
        }

        [HttpPost]
        public async Task<IActionResult> SetTier(int id, CustomerTier tier)
        {
            await _userService.UpdateCustomerTierAsync(id, tier);
            TempData["SuccessMessage"] = "Bayi seviyesi güncellendi.";
            return RedirectToAction("Details", new { id });
        }

        [HttpPost]
        public async Task<IActionResult> ApproveFromDetails(int id, CustomerTier tier, decimal? creditLimit)
        {
            await _userService.ApproveDealerAsync(id);
            
            if (tier != CustomerTier.Standard)
            {
                await _userService.UpdateCustomerTierAsync(id, tier);
            }
            
            if (creditLimit.HasValue && creditLimit.Value > 0)
            {
                await _userService.UpdateCreditLimitAsync(id, creditLimit.Value);
            }

            TempData["SuccessMessage"] = "Bayi başvurusu onaylandı ve ayarlar kaydedildi.";
            return RedirectToAction("Index");
        }
    }
}
