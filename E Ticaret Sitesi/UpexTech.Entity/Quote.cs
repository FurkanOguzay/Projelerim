namespace UpexTech.Entity
{
    public enum QuoteStatus
    {
        Draft = 1,           // Taslak
        Pending = 2,         // Müşteri Onayı Bekliyor
        ManagerApproval = 3, // Yönetici Onayı Bekliyor
        Approved = 4,        // Onaylandı
        Rejected = 5,        // Reddedildi
        Expired = 6,         // Süresi Doldu
        Converted = 7        // Siparişe Dönüştü
    }

    public class Quote : BaseEntity
    {
        public string QuoteNumber { get; set; } = string.Empty;
        public QuoteStatus Status { get; set; } = QuoteStatus.Draft;
        public DateTime ValidUntil { get; set; }
        public decimal TotalAmount { get; set; }
        public string? Notes { get; set; }

        // Foreign Keys
        public int UserId { get; set; }

        // Navigation Properties
        public virtual User User { get; set; } = null!;
        public virtual ICollection<QuoteItem> Items { get; set; } = new List<QuoteItem>();
    }

    public class QuoteItem : BaseEntity
    {
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal DiscountPercentage { get; set; }
        public decimal TotalPrice => Quantity * UnitPrice * (1 - DiscountPercentage / 100);

        // Foreign Keys
        public int QuoteId { get; set; }
        public int ProductId { get; set; }

        // Navigation Properties
        public virtual Quote Quote { get; set; } = null!;
        public virtual Product Product { get; set; } = null!;
    }
}
