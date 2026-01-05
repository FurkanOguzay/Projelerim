namespace UpexTech.Business.DTOs
{
    /// <summary>
    /// Genel Performans özeti - KPI kartları için
    /// </summary>
    public class GeneralPerformanceSummaryDto
    {
        public decimal NetRevenue { get; set; }           // Net Ciro
        public decimal RevenueChange { get; set; }        // Düne göre değişim yüzdesi
        public int OrderCount { get; set; }               // Sipariş Adedi
        public int ViewCount { get; set; }                // Görüntülenme
        public decimal ConversionRate { get; set; }       // Sepete Atma / Satış Oranı
    }

    /// <summary>
    /// Satış grafiği verileri - Line Chart için
    /// </summary>
    public class SalesChartDataDto
    {
        public string Label { get; set; } = string.Empty;  // Saat/Gün/Hafta/Ay etiketi
        public decimal CurrentPeriod { get; set; }          // Bu dönem
        public decimal PreviousPeriod { get; set; }         // Önceki dönem
    }

    /// <summary>
    /// İl bazında satış dağılımı
    /// </summary>
    public class CitySalesDto
    {
        public int Rank { get; set; }
        public string CityName { get; set; } = string.Empty;
        public decimal Sales { get; set; }
        public decimal Percentage { get; set; }
    }

    /// <summary>
    /// Platform dağılımı - Pie Chart için
    /// </summary>
    public class PlatformDistributionDto
    {
        public decimal MobileWebPercentage { get; set; }
        public decimal DesktopPercentage { get; set; }
    }

    /// <summary>
    /// Ürün performans tablosu için detaylı veriler
    /// </summary>
    public class ProductPerformanceDto
    {
        public int Rank { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public decimal NetRevenue { get; set; }           // Net Ciro
        public int SalesCount { get; set; }               // Net Satış Adedi
        public decimal UnitPrice { get; set; }            // Birim Fiyat
        public decimal ReturnRate { get; set; }           // İade Oranı (%)
        public int CurrentStock { get; set; }             // Güncel Stok
        public int CartAdditions { get; set; }            // Sepete Eklenme Sayısı
    }

    /// <summary>
    /// Ürün performansı filtre parametreleri
    /// </summary>
    public class ProductPerformanceFilterDto
    {
        public string Segment { get; set; } = "all";       // all, b2b, b2c
        public string SortBy { get; set; } = "revenue";    // revenue, quantity, cart
        public int? CategoryId { get; set; }
        public int? BrandId { get; set; }
        public string? SearchQuery { get; set; }
    }
}
