using UpexTech.Entity;

namespace UpexTech.Web.Models
{
    /// <summary>
    /// Raporlarım sayfası için ViewModel - B2B ve B2C için farklı veriler içerir
    /// </summary>
    public class ReportsViewModel
    {
        public UserRole UserRole { get; set; } = UserRole.B2C;
        
        // ============ B2B (Bayi) Finansal Verileri ============
        /// <summary>Cari Bakiye (Borç - Alacak)</summary>
        public decimal Balance { get; set; }
        
        /// <summary>Toplam Borç</summary>
        public decimal TotalDebit { get; set; }
        
        /// <summary>Toplam Alacak (Ödenen)</summary>
        public decimal TotalCredit { get; set; }
        
        /// <summary>Vadesi geçmiş ödeme var mı?</summary>
        public bool HasOverduePayments { get; set; }
        
        /// <summary>Son işlem tarihi</summary>
        public DateTime? LastTransactionDate { get; set; }
        
        // ============ B2B - Figma Dashboard Ek Verileri ============
        /// <summary>Bu ayki toplam alım tutarı</summary>
        public decimal ThisMonthPurchase { get; set; }
        
        /// <summary>Bekleyen iade tutarı</summary>
        public decimal PendingReturns { get; set; }
        
        /// <summary>Bekleyen iade sayısı</summary>
        public int PendingReturnCount { get; set; }
        
        /// <summary>Bayi iskonto oranı</summary>
        public decimal DiscountRate { get; set; }
        
        /// <summary>Bayi seviyesi (Premium, Gold vs.)</summary>
        public string DealerTier { get; set; } = "Standard";
        
        /// <summary>Son 6 ay aylık harcama verileri (grafik için)</summary>
        public List<MonthlySpendingData> MonthlyTrend { get; set; } = new();
        
        /// <summary>Kategori bazlı harcama dağılımı (grafik için)</summary>
        public List<CategorySpendingData> CategoryDistribution { get; set; } = new();
        
        /// <summary>Son cari hesap hareketleri</summary>
        public List<TransactionLineItem> RecentTransactions { get; set; } = new();
        
        // ============ B2C (Müşteri) Alışveriş Özeti ============
        /// <summary>Bu yıl toplam harcama</summary>
        public decimal TotalSpentThisYear { get; set; }
        
        /// <summary>Tüm zamanlar toplam harcama</summary>
        public decimal TotalSpentAllTime { get; set; }
        
        /// <summary>Favori kategori (en çok alışveriş yapılan)</summary>
        public string FavoriteCategory { get; set; } = "Henüz alışveriş yok";
        
        /// <summary>Toplam sipariş sayısı</summary>
        public int TotalOrderCount { get; set; }
        
        /// <summary>Toplam puan</summary>
        public int TotalPoints { get; set; }
        
        // ============ Ortak Veriler ============
        /// <summary>Son sipariş (Kargo takibi için)</summary>
        public Order? LatestOrder { get; set; }
        
        /// <summary>Sipariş durumu özeti</summary>
        public OrderStatusSummary OrderSummary { get; set; } = new();
        
        // ============ B2C - Tekrar Satın Al Ürünleri ============
        public List<ReorderProductViewModel> SuggestedProducts { get; set; } = new();
        
        // ============ B2C - Özet Kartları ============
        /// <summary>Aktif sipariş sayısı</summary>
        public int ActiveOrderCount { get; set; }
        
        /// <summary>Toplam kupon sayısı</summary>
        public int TotalCouponCount { get; set; }
        
        /// <summary>Aktif kupon sayısı</summary>
        public int ActiveCouponCount { get; set; }
        
        /// <summary>Son sipariş detayları (timeline için)</summary>
        public LatestOrderDetails? LatestOrderInfo { get; set; }
    }
    
    /// <summary>
    /// Son sipariş detayları (B2C raporlar sayfası için)
    /// </summary>
    public class LatestOrderDetails
    {
        public int OrderId { get; set; }
        public string OrderNumber { get; set; } = "";
        public DateTime OrderDate { get; set; }
        public OrderStatus Status { get; set; }
        public DateTime? EstimatedDeliveryDate { get; set; }
        public string? EstimatedDeliveryTime { get; set; }
    }
    
    /// <summary>
    /// Sipariş durumu özeti
    /// </summary>
    public class OrderStatusSummary
    {
        public int PendingCount { get; set; }
        public int ShippedCount { get; set; }
        public int DeliveredCount { get; set; }
        public int CancelledCount { get; set; }
    }
    
    /// <summary>
    /// Aylık harcama verisi (grafik için)
    /// </summary>
    public class MonthlySpendingData
    {
        public string Month { get; set; } = "";
        public decimal Amount { get; set; }
    }
    
    /// <summary>
    /// Kategori bazlı harcama (grafik için)
    /// </summary>
    public class CategorySpendingData
    {
        public string CategoryName { get; set; } = "";
        public decimal Amount { get; set; }
        public string Color { get; set; } = "#3B82F6";
    }
    
    /// <summary>
    /// Cari hesap hareket satırı
    /// </summary>
    public class TransactionLineItem
    {
        public DateTime Date { get; set; }
        public string TransactionNo { get; set; } = "";
        public string Description { get; set; } = "";
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
        public decimal RunningBalance { get; set; }
    }
}
