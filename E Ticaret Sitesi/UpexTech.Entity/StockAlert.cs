namespace UpexTech.Entity
{
    public class StockAlert : BaseEntity
    {
        public int ProductId { get; set; }
        public int? ProductVariationId { get; set; }  // Varyasyon için bildirim (opsiyonel)
        public int? UserId { get; set; }  // Giriş yapmış kullanıcı (opsiyonel)
        public string Email { get; set; } = string.Empty;  // Bildirim gönderilecek email
        public bool IsNotified { get; set; }  // Bildirim gönderildi mi?
        public DateTime? NotifiedAt { get; set; }  // Bildirim gönderilme zamanı

        // Navigation Properties
        public virtual Product Product { get; set; } = null!;
        public virtual ProductVariation? ProductVariation { get; set; }
        public virtual User? User { get; set; }
    }
}
