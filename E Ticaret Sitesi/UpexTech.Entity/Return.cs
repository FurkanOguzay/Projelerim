namespace UpexTech.Entity
{
    public enum ReturnStatus
    {
        Pending = 1,          // Onay Bekliyor
        Approved = 2,         // Onaylandı
        Rejected = 3,         // Reddedildi
        InTransit = 4,        // Kargoda
        Received = 5,         // Teslim Alındı
        Refunded = 6,         // İade Edildi
        Disputed = 7          // İhtilaflı
    }

    public enum ReturnReason
    {
        Defective = 1,        // Arızalı Ürün
        WrongProduct = 2,     // Yanlış Ürün
        NotAsDescribed = 3,   // Açıklamaya Uygun Değil
        DamagedInShipping = 4,// Kargoda Hasar
        ChangedMind = 5,      // Fikir Değişikliği
        Other = 6             // Diğer
    }

    public class Return : BaseEntity
    {
        public string ReturnNumber { get; set; } = string.Empty;
        public ReturnStatus Status { get; set; } = ReturnStatus.Pending;
        public ReturnReason Reason { get; set; }
        public string? ReasonDescription { get; set; }
        public int Quantity { get; set; }
        public decimal RefundAmount { get; set; }
        public string? TrackingNumber { get; set; }
        public string? AttachmentPath { get; set; }
        public string? AdminNotes { get; set; }

        // Foreign Keys
        public int OrderId { get; set; }
        public int OrderItemId { get; set; }
        public int UserId { get; set; }

        // Navigation Properties
        public virtual Order Order { get; set; } = null!;
        public virtual OrderItem OrderItem { get; set; } = null!;
        public virtual User User { get; set; } = null!;
    }
}
