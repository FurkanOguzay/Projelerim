namespace UpexTech.Entity
{
    public enum RoundingMethod
    {
        None = 0,           // Yuvarlama Yok
        Ending90 = 1,       // Sonu .90 ile biten
        Ending99 = 2,       // Sonu .99 ile biten
        NearestFive = 3     // En Yakın 5 TL
    }

    public class PriceList : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int DisplayOrder { get; set; }
        
        // Hesaplama alanları
        public int? BasePriceListId { get; set; }    // Taban fiyat listesi (null ise satınalma fiyatı)
        public decimal Factor { get; set; } = 1.0m;  // Çarpan
        public RoundingMethod Rounding { get; set; } = RoundingMethod.None;

        // Navigation Properties
        public virtual PriceList? BasePriceList { get; set; }
        public virtual ICollection<CustomerGroupPriceList> CustomerGroups { get; set; } = new List<CustomerGroupPriceList>();
    }

    public class CustomerGroup : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal DiscountPercentage { get; set; }

        // Navigation Properties
        public virtual ICollection<CustomerGroupPriceList> PriceLists { get; set; } = new List<CustomerGroupPriceList>();
        // User relationship disabled until migration is applied
        // public virtual ICollection<User> Users { get; set; } = new List<User>();
    }

    public class CustomerGroupPriceList : BaseEntity
    {
        public int CustomerGroupId { get; set; }
        public int PriceListId { get; set; }

        // Navigation Properties
        public virtual CustomerGroup CustomerGroup { get; set; } = null!;
        public virtual PriceList PriceList { get; set; } = null!;
    }
}
