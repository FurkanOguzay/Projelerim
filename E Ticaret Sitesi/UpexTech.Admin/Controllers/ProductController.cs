using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using UpexTech.Business.Services;
using UpexTech.Entity;

namespace UpexTech.Admin.Controllers
{
    [Authorize]
    public class ProductController : AdminBaseController
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly IBrandService _brandService;
        private readonly IDeviceModelService _deviceModelService;

        public ProductController(
            IProductService productService,
            ICategoryService categoryService,
            IBrandService brandService,
            IDeviceModelService deviceModelService)
        {
            _productService = productService;
            _categoryService = categoryService;
            _brandService = brandService;
            _deviceModelService = deviceModelService;
        }

        public async Task<IActionResult> Index(string? sku, string? barcode, int? deviceModelId, string? search, bool? criticalStockOnly, int page = 1)
        {
            var pageSize = 20;
            var (products, totalCount) = await _productService.GetProductsFilteredAsync(
                page, pageSize, sku, barcode, deviceModelId, search, criticalStockOnly);

            // Cihaz modeli dropdown için veri
            var deviceModels = await _deviceModelService.GetAllAsync();
            ViewBag.DeviceModels = new SelectList(deviceModels.Where(d => d.Level == 2), "Id", "Name", deviceModelId);
            
            // Filtre değerlerini ViewBag'e aktar
            ViewBag.CurrentSku = sku;
            ViewBag.CurrentBarcode = barcode;
            ViewBag.CurrentDeviceModelId = deviceModelId;
            ViewBag.CurrentSearch = search;
            ViewBag.CriticalStockOnly = criticalStockOnly;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalCount / pageSize);
            ViewBag.TotalCount = totalCount;

            return View(products);
        }

        public async Task<IActionResult> Create()
        {
            await PopulateDropdowns();
            return View(new Product());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product, int[]? compatibleModelIds)
        {
            // Debug: Log ModelState errors
            if (!ModelState.IsValid)
            {
                Console.WriteLine("[ADMIN] ModelState is INVALID. Errors:");
                foreach (var state in ModelState)
                {
                    foreach (var error in state.Value.Errors)
                    {
                        Console.WriteLine($"  - {state.Key}: {error.ErrorMessage}");
                    }
                }
            }
            else
            {
                Console.WriteLine("[ADMIN] ModelState is VALID. Creating product...");
            }
            
            if (ModelState.IsValid)
            {
                var createdProduct = await _productService.CreateProductAsync(product);
                Console.WriteLine($"[ADMIN] Product created with ID: {createdProduct.Id}");
                
                // Uyumlu modelleri kaydet
                if (compatibleModelIds != null && compatibleModelIds.Length > 0)
                {
                    await _productService.UpdateCompatibleModelsAsync(createdProduct.Id, compatibleModelIds.ToList());
                }
                
                TempData["SuccessMessage"] = "Ürün başarıyla eklendi.";
                return RedirectToAction("Index");
            }
            await PopulateDropdowns(product.CategoryId, product.BrandId);
            return View(product);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var product = await _productService.GetProductByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            await PopulateDropdowns(product.CategoryId, product.BrandId);
            
            // Uyumlu modelleri ViewBag'e ekle
            var compatibleModels = await _productService.GetProductCompatibleModelsAsync(id);
            ViewBag.SelectedModelIds = compatibleModels.Select(m => m.Id).ToList();
            
            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Product product, int[]? compatibleModelIds)
        {
            if (id != product.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                product.UpdatedAt = DateTime.Now;
                await _productService.UpdateProductAsync(product);
                
                // Uyumlu modelleri güncelle
                await _productService.UpdateCompatibleModelsAsync(id, (compatibleModelIds ?? Array.Empty<int>()).ToList());
                
                TempData["SuccessMessage"] = "Ürün başarıyla güncellendi.";
                return RedirectToAction("Index");
            }
            await PopulateDropdowns(product.CategoryId, product.BrandId);
            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            await _productService.DeleteProductAsync(id);
            TempData["SuccessMessage"] = "Ürün başarıyla silindi.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var product = await _productService.GetProductByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            
            product.IsActive = !product.IsActive;
            product.UpdatedAt = DateTime.Now;
            await _productService.UpdateProductAsync(product);
            
            return Json(new { success = true, isActive = product.IsActive });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> QuickStock(int id, int stock)
        {
            var product = await _productService.GetProductByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            
            product.Stock = stock;
            product.UpdatedAt = DateTime.Now;
            await _productService.UpdateProductAsync(product);
            
            var isCritical = stock <= product.CriticalStockLevel;
            return Json(new { success = true, stock = stock, isCritical = isCritical });
        }

        [HttpGet]
        public async Task<IActionResult> GetBrandsByCategory(int categoryId)
        {
            var brands = await _brandService.GetBrandsByCategoryAsync(categoryId);
            return Json(brands.Select(b => new { value = b.Id, text = b.Name }));
        }

        [HttpGet]
        public async Task<IActionResult> GetDeviceModelChildren(int parentId)
        {
            var children = await _deviceModelService.GetChildrenAsync(parentId);
            return Json(children.Select(c => new { id = c.Id, name = c.Name, level = c.Level }));
        }

        private async Task PopulateDropdowns(int? selectedCategoryId = null, int? selectedBrandId = null)
        {
            var categories = await _categoryService.GetAllCategoriesAsync();
            var brands = await _brandService.GetAllBrandsAsync();
            var deviceModels = await _deviceModelService.GetModelTreeAsync();

            ViewBag.Categories = new SelectList(categories, "Id", "Name", selectedCategoryId);
            ViewBag.Brands = new SelectList(brands, "Id", "Name", selectedBrandId);
            ViewBag.DeviceModels = deviceModels;
        }
    }
}

