namespace UpexTech.Entity
{
    public class Product : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Image { get; set; }
        public int Stock { get; set; }
        public decimal PriceB2C { get; set; }  // Perakende fiyatı
        public decimal PriceB2B { get; set; }  // Bayi fiyatı
        public double Rating { get; set; }
        public int ReviewCount { get; set; }
        public bool IsPopular { get; set; }
        public bool IsImmediateDelivery { get; set; }

        // Yeni Katalog Alanları
        public string? SKU { get; set; }           // Stok Kodu
        public string? Barcode { get; set; }       // Barkod
        public decimal PurchasePrice { get; set; } // Alış Fiyatı
        public int CriticalStockLevel { get; set; } = 10; // Kritik stok seviyesi

        // Filtre Alanları
        public string? Material { get; set; }  // Malzeme: Silikon, Deri, Plastik, TPU, vb.
        public string? Color { get; set; }     // Renk: Siyah, Beyaz, Mavi, vb.

        // PDP Alanları
        public string? TechnicalSpecs { get; set; }  // JSON formatında teknik özellikler

        // Foreign Keys
        public int CategoryId { get; set; }
        public int BrandId { get; set; }

        // Navigation Properties
        public virtual Category? Category { get; set; }
        public virtual Brand? Brand { get; set; }
        public virtual ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        public virtual ICollection<ProductDeviceModel> CompatibleModels { get; set; } = new List<ProductDeviceModel>();
        
        // PDP Navigation Properties
        public virtual ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
        public virtual ICollection<ProductVariation> Variations { get; set; } = new List<ProductVariation>();
        public virtual ICollection<StockAlert> StockAlerts { get; set; } = new List<StockAlert>();
        public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
    }
}
