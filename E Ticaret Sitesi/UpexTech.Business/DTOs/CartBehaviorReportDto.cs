namespace UpexTech.Business.DTOs
{
    public class CartBehaviorReportDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;  // Product ID kullanılacak
        public string CategoryName { get; set; } = string.Empty;
        
        // Sepet Metrikleri
        public int GrossCartAddCount { get; set; }  // Brüt Sepete Eklenme (aktif sepetteki toplam)
        public int CurrentCartUserCount { get; set; }  // Şu an kaç kullanıcının sepetinde
        
        // Favori Metrikleri
        public int CurrentFavoriteCount { get; set; }  // Aktif favori sayısı
        
        // Satış Metrikleri
        public int NetSalesCount { get; set; }  // Net Satış Adedi (tamamlanmış siparişlerden)
        public decimal NetRevenue { get; set; }  // Net Ciro
        
        // Stok Bilgisi
        public int CurrentStock { get; set; }  // Güncel Stok
    }
}
