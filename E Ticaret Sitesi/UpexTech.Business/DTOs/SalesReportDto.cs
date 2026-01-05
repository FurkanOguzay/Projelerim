namespace UpexTech.Business.DTOs
{
    /// <summary>
    /// Satış raporu özet verileri
    /// </summary>
    public class SalesReportSummaryDto
    {
        public decimal TotalRevenue { get; set; }       // Toplam Ciro
        public int TotalOrders { get; set; }            // Sipariş Sayısı
        public int TotalProductsSold { get; set; }      // Satılan Ürün Adedi
        public decimal AverageOrderValue { get; set; }  // Ortalama Sipariş Tutarı
    }

    /// <summary>
    /// Günlük/Aylık trend verisi (Line Chart için)
    /// </summary>
    public class SalesTrendDto
    {
        public string Date { get; set; } = string.Empty;  // Tarih etiketi
        public decimal Revenue { get; set; }               // Ciro
        public int OrderCount { get; set; }                // Sipariş sayısı
    }

    /// <summary>
    /// En çok satan ürünler (Top Products tablosu için)
    /// </summary>
    public class TopProductDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public int SalesCount { get; set; }
        public decimal Revenue { get; set; }
    }

    /// <summary>
    /// Kategori dağılımı (Pie Chart için)
    /// </summary>
    public class CategoryDistributionDto
    {
        public string CategoryName { get; set; } = string.Empty;
        public decimal Revenue { get; set; }
        public decimal Percentage { get; set; }
    }

    /// <summary>
    /// Stok devir hızı metrikleri
    /// </summary>
    public class StockTurnoverDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public int CurrentStock { get; set; }
        public int TotalSold { get; set; }           // Son dönemde satılan adet
        public decimal TurnoverRate { get; set; }    // Devir hızı (satılan/ortalama stok)
        public string TurnoverStatus { get; set; } = string.Empty; // Hızlı, Normal, Yavaş
    }
}
