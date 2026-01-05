using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using UpexTech.Business.Services;
using UpexTech.Entity;

namespace UpexTech.Admin.Controllers
{
    [Authorize]
    public class BannerController : AdminBaseController
    {
        private readonly IBannerService _bannerService;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public BannerController(IBannerService bannerService, IWebHostEnvironment webHostEnvironment)
        {
            _bannerService = bannerService;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<IActionResult> Index(BannerPosition? position, bool? activeOnly)
        {
            var banners = await _bannerService.GetAllBannersAsync();

            if (position.HasValue)
            {
                banners = banners.Where(b => b.Position == position.Value);
            }

            if (activeOnly == true)
            {
                var now = DateTime.Now;
                banners = banners.Where(b => b.IsActive && b.StartDate <= now && b.EndDate >= now);
            }

            ViewBag.Positions = GetPositionSelectList(position);
            ViewBag.CurrentPosition = position;
            ViewBag.ActiveOnly = activeOnly;

            return View(banners.ToList());
        }

        public IActionResult Create()
        {
            ViewBag.Positions = GetPositionSelectList();
            return View(new Banner 
            { 
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddMonths(1)
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Banner banner)
        {
            if (ModelState.IsValid)
            {
                await _bannerService.CreateBannerAsync(banner);
                TempData["SuccessMessage"] = "Banner başarıyla oluşturuldu.";
                return RedirectToAction("Index");
            }

            ViewBag.Positions = GetPositionSelectList(banner.Position);
            return View(banner);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var banner = await _bannerService.GetBannerByIdAsync(id);
            if (banner == null)
            {
                return NotFound();
            }

            ViewBag.Positions = GetPositionSelectList(banner.Position);
            return View(banner);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Banner banner)
        {
            if (id != banner.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                banner.UpdatedAt = DateTime.Now;
                await _bannerService.UpdateBannerAsync(banner);
                TempData["SuccessMessage"] = "Banner başarıyla güncellendi.";
                return RedirectToAction("Index");
            }

            ViewBag.Positions = GetPositionSelectList(banner.Position);
            return View(banner);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            await _bannerService.DeleteBannerAsync(id);
            TempData["SuccessMessage"] = "Banner başarıyla silindi.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var banner = await _bannerService.GetBannerByIdAsync(id);
            if (banner == null)
            {
                return NotFound();
            }

            banner.IsActive = !banner.IsActive;
            banner.UpdatedAt = DateTime.Now;
            await _bannerService.UpdateBannerAsync(banner);

            return Json(new { success = true, isActive = banner.IsActive });
        }

        [HttpPost]
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            try
            {
                var imageUrl = await _bannerService.UploadImageAsync(file, _webHostEnvironment.WebRootPath);
                return Json(new { success = true, imageUrl });
            }
            catch (ArgumentException ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
            catch (Exception)
            {
                return Json(new { success = false, error = "Görsel yüklenirken bir hata oluştu." });
            }
        }

        private SelectList GetPositionSelectList(BannerPosition? selectedPosition = null)
        {
            var positions = new List<object>
            {
                new { Value = (int)BannerPosition.HomePage, Text = "Ana Sayfa" },
                new { Value = (int)BannerPosition.CategoryTop, Text = "Kategori Üstü" },
                new { Value = (int)BannerPosition.ProductDetail, Text = "Ürün Detay" },
                new { Value = (int)BannerPosition.Checkout, Text = "Ödeme Sayfası" }
            };

            return new SelectList(positions, "Value", "Text", selectedPosition.HasValue ? (int)selectedPosition.Value : null);
        }
    }
}
