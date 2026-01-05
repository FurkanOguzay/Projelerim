namespace UpexTech.Entity
{
    /// <summary>
    /// Banner entity for managing campaign visuals
    /// </summary>
    public class Banner : BaseEntity
    {
        public string Title { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public string? TargetUrl { get; set; }
        public BannerPosition Position { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int DisplayOrder { get; set; } = 0;
    }

    /// <summary>
    /// Banner position enum for placement options
    /// </summary>
    public enum BannerPosition
    {
        HomePage = 1,       // Ana Sayfa
        CategoryTop = 2,    // Kategori Üstü
        ProductDetail = 3,  // Ürün Detay
        Checkout = 4        // Ödeme Sayfası
    }
}
