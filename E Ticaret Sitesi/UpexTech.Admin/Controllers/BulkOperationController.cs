using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UpexTech.Business.Services;
using UpexTech.Entity;

namespace UpexTech.Admin.Controllers
{
    [Authorize]
    public class BulkOperationController : AdminBaseController
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly IBrandService _brandService;

        public BulkOperationController(
            IProductService productService,
            ICategoryService categoryService,
            IBrandService brandService)
        {
            _productService = productService;
            _categoryService = categoryService;
            _brandService = brandService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> DownloadProductTemplate()
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Ürünler");

            // Başlık satırı
            var headers = new[] { "Ürün Adı*", "Açıklama", "SKU*", "Barkod", "Kategori ID*", "Marka ID*", 
                                  "Stok*", "Alış Fiyatı*", "B2C Fiyat*", "B2B Fiyat*", "Kritik Stok Seviyesi", "Aktif (1/0)" };
            
            for (int i = 0; i < headers.Length; i++)
            {
                worksheet.Cell(1, i + 1).Value = headers[i];
                worksheet.Cell(1, i + 1).Style.Font.Bold = true;
                worksheet.Cell(1, i + 1).Style.Fill.BackgroundColor = XLColor.LightGray;
            }

            // Örnek veri
            worksheet.Cell(2, 1).Value = "iPhone 13 Ekran";
            worksheet.Cell(2, 2).Value = "Orijinal kalitede ekran";
            worksheet.Cell(2, 3).Value = "IP13-SCR-001";
            worksheet.Cell(2, 4).Value = "8680000000001";
            worksheet.Cell(2, 5).Value = 1;
            worksheet.Cell(2, 6).Value = 1;
            worksheet.Cell(2, 7).Value = 100;
            worksheet.Cell(2, 8).Value = 500;
            worksheet.Cell(2, 9).Value = 999;
            worksheet.Cell(2, 10).Value = 800;
            worksheet.Cell(2, 11).Value = 10;
            worksheet.Cell(2, 12).Value = 1;

            // Kategori ve Marka bilgisi için referans sayfası
            var refSheet = workbook.Worksheets.Add("Referanslar");
            refSheet.Cell(1, 1).Value = "Kategoriler";
            refSheet.Cell(1, 1).Style.Font.Bold = true;
            refSheet.Cell(1, 2).Value = "ID";
            refSheet.Cell(1, 2).Style.Font.Bold = true;

            var categories = await _categoryService.GetAllCategoriesAsync();
            int row = 2;
            foreach (var cat in categories)
            {
                refSheet.Cell(row, 1).Value = cat.Name;
                refSheet.Cell(row, 2).Value = cat.Id;
                row++;
            }

            refSheet.Cell(1, 4).Value = "Markalar";
            refSheet.Cell(1, 4).Style.Font.Bold = true;
            refSheet.Cell(1, 5).Value = "ID";
            refSheet.Cell(1, 5).Style.Font.Bold = true;

            var brands = await _brandService.GetAllBrandsAsync();
            row = 2;
            foreach (var brand in brands)
            {
                refSheet.Cell(row, 4).Value = brand.Name;
                refSheet.Cell(row, 5).Value = brand.Id;
                row++;
            }

            worksheet.Columns().AdjustToContents();
            refSheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;

            return File(stream.ToArray(), 
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", 
                "urun_sablonu.xlsx");
        }

        [HttpGet]
        public IActionResult DownloadStockTemplate()
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Stok Güncelleme");

            // Başlık satırı
            worksheet.Cell(1, 1).Value = "Ürün ID*";
            worksheet.Cell(1, 2).Value = "SKU";
            worksheet.Cell(1, 3).Value = "Yeni Stok*";
            
            for (int i = 1; i <= 3; i++)
            {
                worksheet.Cell(1, i).Style.Font.Bold = true;
                worksheet.Cell(1, i).Style.Fill.BackgroundColor = XLColor.LightGray;
            }

            // Örnek veri
            worksheet.Cell(2, 1).Value = 1;
            worksheet.Cell(2, 2).Value = "IP13-SCR-001";
            worksheet.Cell(2, 3).Value = 150;

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;

            return File(stream.ToArray(), 
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", 
                "stok_guncelleme_sablonu.xlsx");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ImportProducts(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                TempData["ErrorMessage"] = "Lütfen bir dosya seçin.";
                return RedirectToAction("Index");
            }

            try
            {
                using var stream = file.OpenReadStream();
                using var workbook = new XLWorkbook(stream);
                var worksheet = workbook.Worksheet(1);

                var products = new List<Product>();
                var rows = worksheet.RowsUsed().Skip(1); // Başlık satırını atla

                foreach (var row in rows)
                {
                    var product = new Product
                    {
                        Name = row.Cell(1).GetString(),
                        Description = row.Cell(2).GetString(),
                        SKU = row.Cell(3).GetString(),
                        Barcode = row.Cell(4).GetString(),
                        CategoryId = row.Cell(5).GetValue<int>(),
                        BrandId = row.Cell(6).GetValue<int>(),
                        Stock = row.Cell(7).GetValue<int>(),
                        PurchasePrice = row.Cell(8).GetValue<decimal>(),
                        PriceB2C = row.Cell(9).GetValue<decimal>(),
                        PriceB2B = row.Cell(10).GetValue<decimal>(),
                        CriticalStockLevel = row.Cell(11).IsEmpty() ? 10 : row.Cell(11).GetValue<int>(),
                        IsActive = row.Cell(12).IsEmpty() || row.Cell(12).GetValue<int>() == 1,
                        CreatedAt = DateTime.Now
                    };

                    products.Add(product);
                }

                await _productService.BulkCreateAsync(products);
                TempData["SuccessMessage"] = $"{products.Count} ürün başarıyla içeri aktarıldı.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"İçeri aktarma hatası: {ex.Message}";
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ImportStock(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                TempData["ErrorMessage"] = "Lütfen bir dosya seçin.";
                return RedirectToAction("Index");
            }

            try
            {
                using var stream = file.OpenReadStream();
                using var workbook = new XLWorkbook(stream);
                var worksheet = workbook.Worksheet(1);

                var stockUpdates = new Dictionary<int, int>();
                var rows = worksheet.RowsUsed().Skip(1); // Başlık satırını atla

                foreach (var row in rows)
                {
                    var productId = row.Cell(1).GetValue<int>();
                    var newStock = row.Cell(3).GetValue<int>();
                    stockUpdates[productId] = newStock;
                }

                await _productService.BulkUpdateStockAsync(stockUpdates);
                TempData["SuccessMessage"] = $"{stockUpdates.Count} ürünün stok bilgisi güncellendi.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Stok güncelleme hatası: {ex.Message}";
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> ExportProducts()
        {
            var products = await _productService.GetAllProductsForAdminAsync();

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Ürünler");

            // Başlık satırı
            var headers = new[] { "ID", "Ürün Adı", "SKU", "Barkod", "Kategori", "Marka", 
                                  "Stok", "Kritik Stok", "Alış Fiyatı", "B2C Fiyat", "B2B Fiyat", "Durum" };
            
            for (int i = 0; i < headers.Length; i++)
            {
                worksheet.Cell(1, i + 1).Value = headers[i];
                worksheet.Cell(1, i + 1).Style.Font.Bold = true;
                worksheet.Cell(1, i + 1).Style.Fill.BackgroundColor = XLColor.LightGray;
            }

            int row = 2;
            foreach (var product in products)
            {
                worksheet.Cell(row, 1).Value = product.Id;
                worksheet.Cell(row, 2).Value = product.Name;
                worksheet.Cell(row, 3).Value = product.SKU ?? "";
                worksheet.Cell(row, 4).Value = product.Barcode ?? "";
                worksheet.Cell(row, 5).Value = product.Category?.Name ?? "";
                worksheet.Cell(row, 6).Value = product.Brand?.Name ?? "";
                worksheet.Cell(row, 7).Value = product.Stock;
                worksheet.Cell(row, 8).Value = product.CriticalStockLevel;
                worksheet.Cell(row, 9).Value = product.PurchasePrice;
                worksheet.Cell(row, 10).Value = product.PriceB2C;
                worksheet.Cell(row, 11).Value = product.PriceB2B;
                worksheet.Cell(row, 12).Value = product.IsActive ? "Aktif" : "Pasif";

                // Kritik stok altındakileri kırmızı yap
                if (product.Stock <= product.CriticalStockLevel)
                {
                    worksheet.Cell(row, 7).Style.Font.FontColor = XLColor.Red;
                    worksheet.Cell(row, 7).Style.Font.Bold = true;
                }

                row++;
            }

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;

            var fileName = $"urunler_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
            return File(stream.ToArray(), 
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", 
                fileName);
        }
    }
}
