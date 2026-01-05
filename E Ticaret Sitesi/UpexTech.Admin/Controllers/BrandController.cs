using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using UpexTech.Business.Services;
using UpexTech.Entity;

namespace UpexTech.Admin.Controllers
{
    [Authorize]
    public class BrandController : AdminBaseController
    {
        private readonly IBrandService _brandService;
        private readonly ICategoryService _categoryService;

        public BrandController(IBrandService brandService, ICategoryService categoryService)
        {
            _brandService = brandService;
            _categoryService = categoryService;
        }

        public async Task<IActionResult> Index()
        {
            var brands = await _brandService.GetAllBrandsAsync();
            return View(brands);
        }

        public async Task<IActionResult> Create()
        {
            await PopulateCategories();
            return View(new Brand());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Brand brand)
        {
            if (ModelState.IsValid)
            {
                await _brandService.CreateBrandAsync(brand);
                TempData["SuccessMessage"] = "Marka başarıyla eklendi.";
                return RedirectToAction("Index");
            }
            await PopulateCategories(brand.CategoryId);
            return View(brand);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var brand = await _brandService.GetBrandByIdAsync(id);
            if (brand == null)
            {
                return NotFound();
            }
            await PopulateCategories(brand.CategoryId);
            return View(brand);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Brand brand)
        {
            if (id != brand.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                brand.UpdatedAt = DateTime.Now;
                await _brandService.UpdateBrandAsync(brand);
                TempData["SuccessMessage"] = "Marka başarıyla güncellendi.";
                return RedirectToAction("Index");
            }
            await PopulateCategories(brand.CategoryId);
            return View(brand);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            await _brandService.DeleteBrandAsync(id);
            TempData["SuccessMessage"] = "Marka başarıyla silindi.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var brand = await _brandService.GetBrandByIdAsync(id);
            if (brand == null)
            {
                return NotFound();
            }

            brand.IsActive = !brand.IsActive;
            brand.UpdatedAt = DateTime.Now;
            await _brandService.UpdateBrandAsync(brand);

            return Json(new { success = true, isActive = brand.IsActive });
        }

        private async Task PopulateCategories(int? selectedCategoryId = null)
        {
            var categories = await _categoryService.GetAllCategoriesAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Name", selectedCategoryId);
        }
    }
}
