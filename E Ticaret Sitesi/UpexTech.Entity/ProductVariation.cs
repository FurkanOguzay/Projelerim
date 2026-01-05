namespace UpexTech.Entity
{
    public enum VariationType
    {
        Color,      // Renk
        Capacity,   // Kapasite (mAh, GB vb.)
        Compatibility  // Uyumluluk (iPhone 14, iPhone 15 vb.)
    }

    public class ProductVariation : BaseEntity
    {
        public int ProductId { get; set; }
        public VariationType VariationType { get; set; }
        public string VariationValue { get; set; } = string.Empty;  // "Siyah", "4000mAh", "iPhone 15"
        public string? ColorCode { get; set; }  // Renk için hex kodu: "#000000"
        public int Stock { get; set; }
        public decimal PriceAdjustment { get; set; }  // Ana fiyata eklenen/çıkarılan tutar
        public string? SKU { get; set; }  // Varyasyon SKU'su
        public string? ImageUrl { get; set; }  // Varyasyona özel görsel

        // Navigation Property
        public virtual Product Product { get; set; } = null!;
    }
}
