using Microsoft.AspNetCore.Mvc;
using UpexTech.Business.Services;
using UpexTech.Business.DTOs;

namespace UpexTech.Admin.Controllers
{
    public class ReportController : AdminBaseController
    {
        private readonly ICartMetricsService _cartMetricsService;
        private readonly IExcelService _excelService;
        private readonly ISalesReportService _salesReportService;
        private readonly IPerformanceReportService _performanceReportService;

        public ReportController(
            ICartMetricsService cartMetricsService,
            IExcelService excelService,
            ISalesReportService salesReportService,
            IPerformanceReportService performanceReportService)
        {
            _cartMetricsService = cartMetricsService;
            _excelService = excelService;
            _salesReportService = salesReportService;
            _performanceReportService = performanceReportService;
        }

        // Ana rapor sayfası
        public IActionResult Index()
        {
            return View();
        }

        // Detaylı Satış Analizi sayfası
        public IActionResult SalesReport()
        {
            return View();
        }

        // Genel Performans sayfası
        public IActionResult GeneralPerformance()
        {
            return View();
        }

        // Ürün Performansı sayfası
        public IActionResult ProductPerformance()
        {
            return View();
        }

        #region Performans Detayları API

        // Genel performans özeti - KPI kartları için
        [HttpGet]
        public async Task<IActionResult> GetGeneralPerformanceSummary(string segment = "all", DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                var start = startDate ?? DateTime.Today;
                var end = endDate ?? DateTime.Today.AddDays(1).AddSeconds(-1);

                var summary = await _performanceReportService.GetGeneralPerformanceSummaryAsync(segment, start, end);
                return Json(new { success = true, data = summary });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // Satış grafiği verileri
        [HttpGet]
        public async Task<IActionResult> GetSalesChartData(string segment = "all", DateTime? startDate = null, DateTime? endDate = null, string breakdown = "hourly")
        {
            try
            {
                var start = startDate ?? DateTime.Today;
                var end = endDate ?? DateTime.Today.AddDays(1).AddSeconds(-1);

                var chartData = await _performanceReportService.GetSalesChartDataAsync(segment, start, end, breakdown);
                return Json(new { success = true, data = chartData });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // İl bazında satış dağılımı
        [HttpGet]
        public async Task<IActionResult> GetCitySalesDistribution(string segment = "all", DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                var start = startDate ?? DateTime.Today.AddDays(-30);
                var end = endDate ?? DateTime.Today.AddDays(1).AddSeconds(-1);

                var citySales = await _performanceReportService.GetCitySalesDistributionAsync(segment, start, end);
                return Json(new { success = true, data = citySales });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // Platform dağılımı
        [HttpGet]
        public async Task<IActionResult> GetPlatformDistribution(string segment = "all", DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                var start = startDate ?? DateTime.Today.AddDays(-30);
                var end = endDate ?? DateTime.Today.AddDays(1).AddSeconds(-1);

                var platformData = await _performanceReportService.GetPlatformDistributionAsync(segment, start, end);
                return Json(new { success = true, data = platformData });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // Ürün performans verileri
        [HttpGet]
        public async Task<IActionResult> GetProductPerformanceData(
            string segment = "all",
            string sortBy = "revenue",
            int? categoryId = null,
            int? brandId = null,
            string? search = null)
        {
            try
            {
                var filter = new ProductPerformanceFilterDto
                {
                    Segment = segment,
                    SortBy = sortBy,
                    CategoryId = categoryId,
                    BrandId = brandId,
                    SearchQuery = search
                };

                var products = await _performanceReportService.GetProductPerformanceAsync(filter);
                return Json(new { success = true, data = products });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        #endregion

        #region Müşteri Davranışı Raporu

        // Müşteri davranışı raporu verilerini JSON olarak döner
        [HttpGet]
        public async Task<IActionResult> GetCartBehaviorData()
        {
            try
            {
                var reportData = await _cartMetricsService.GetCartBehaviorReportAsync();
                return Json(new { success = true, data = reportData });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // Excel export
        [HttpGet]
        public async Task<IActionResult> ExportCartBehaviorToExcel()
        {
            try
            {
                var reportData = await _cartMetricsService.GetCartBehaviorReportAsync();

                // Kolon başlıklarını Türkçe olarak tanımla
                var columnMappings = new Dictionary<string, string>
                {
                    { "ProductId", "Ürün ID" },
                    { "ProductName", "Ürün Adı" },
                    { "SKU", "Stok Kodu" },
                    { "CategoryName", "Kategori" },
                    { "GrossCartAddCount", "Brüt Sepete Eklenme" },
                    { "CurrentCartUserCount", "Şu An Kaç Sepette" },
                    { "CurrentFavoriteCount", "Aktif Favori Sayısı" },
                    { "NetSalesCount", "Net Satış Adedi" },
                    { "NetRevenue", "Net Ciro (TL)" },
                    { "CurrentStock", "Güncel Stok" }
                };

                var excelData = _excelService.ExportToExcel(
                    reportData,
                    "Müşteri Davranışı Raporu",
                    columnMappings
                );

                var fileName = $"Musteri_Davranisi_Raporu_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

                return File(excelData, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Excel oluşturulurken hata: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        #endregion

        #region Satış Analizi Raporu

        // Satış özeti - KPI kartları için
        [HttpGet]
        public async Task<IActionResult> GetSalesSummary(DateTime? startDate, DateTime? endDate)
        {
            try
            {
                var start = startDate ?? DateTime.Today.AddDays(-30);
                var end = endDate ?? DateTime.Today.AddDays(1).AddSeconds(-1);

                var summary = await _salesReportService.GetSalesSummaryAsync(start, end);
                return Json(new { success = true, data = summary });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // Satış trendi - Line Chart için
        [HttpGet]
        public async Task<IActionResult> GetSalesTrend(DateTime? startDate, DateTime? endDate)
        {
            try
            {
                var start = startDate ?? DateTime.Today.AddDays(-30);
                var end = endDate ?? DateTime.Today.AddDays(1).AddSeconds(-1);

                var trend = await _salesReportService.GetSalesTrendAsync(start, end);
                return Json(new { success = true, data = trend });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // En çok satanlar - Tablo için
        [HttpGet]
        public async Task<IActionResult> GetTopProducts(DateTime? startDate, DateTime? endDate, int count = 10)
        {
            try
            {
                var start = startDate ?? DateTime.Today.AddDays(-30);
                var end = endDate ?? DateTime.Today.AddDays(1).AddSeconds(-1);

                var topProducts = await _salesReportService.GetTopProductsAsync(start, end, count);
                return Json(new { success = true, data = topProducts });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // Kategori dağılımı - Pie Chart için
        [HttpGet]
        public async Task<IActionResult> GetCategoryDistribution(DateTime? startDate, DateTime? endDate)
        {
            try
            {
                var start = startDate ?? DateTime.Today.AddDays(-30);
                var end = endDate ?? DateTime.Today.AddDays(1).AddSeconds(-1);

                var distribution = await _salesReportService.GetCategoryDistributionAsync(start, end);
                return Json(new { success = true, data = distribution });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // Stok devir hızı
        [HttpGet]
        public async Task<IActionResult> GetStockTurnover(int days = 30)
        {
            try
            {
                var stockTurnover = await _salesReportService.GetStockTurnoverAsync(days);
                return Json(new { success = true, data = stockTurnover });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // Satış raporu Excel export
        [HttpGet]
        public async Task<IActionResult> ExportSalesReportToExcel(DateTime? startDate, DateTime? endDate)
        {
            try
            {
                var start = startDate ?? DateTime.Today.AddDays(-30);
                var end = endDate ?? DateTime.Today.AddDays(1).AddSeconds(-1);

                var topProducts = await _salesReportService.GetTopProductsAsync(start, end, 100);

                var columnMappings = new Dictionary<string, string>
                {
                    { "ProductId", "Ürün ID" },
                    { "ProductName", "Ürün Adı" },
                    { "CategoryName", "Kategori" },
                    { "SalesCount", "Satış Adedi" },
                    { "Revenue", "Ciro (TL)" }
                };

                var excelData = _excelService.ExportToExcel(
                    topProducts,
                    "Satış Raporu",
                    columnMappings
                );

                var fileName = $"Satis_Raporu_{start:yyyyMMdd}_{end:yyyyMMdd}.xlsx";

                return File(excelData, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Excel oluşturulurken hata: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        #endregion
    }
}
