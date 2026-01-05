namespace UpexTech.Entity
{
    public enum PaymentMethod
    {
        CreditCard = 1,      // Kredi Kartı
        BankTransfer = 2,    // Banka Havalesi/EFT
        IyzicoPOS = 3,       // Iyzico POS
        CashOnDelivery = 4   // Kapıda Ödeme
    }

    public enum PaymentStatus
    {
        Pending = 1,         // Beklemede
        Completed = 2,       // Başarılı
        Failed = 3,          // Başarısız
        Refunded = 4         // İade Edildi
    }

    public enum PaymentChannel
    {
        B2C = 1,             // Bireysel
        B2B = 2              // Kurumsal
    }

    public class Payment : BaseEntity
    {
        public int UserId { get; set; }
        public int? OrderId { get; set; }
        public decimal Amount { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
        public PaymentChannel Channel { get; set; }
        public DateTime PaymentDate { get; set; } = DateTime.Now;
        public string? ReferenceNumber { get; set; }
        public string? Description { get; set; }
        public string? BankName { get; set; }
        public string? AccountName { get; set; }
        public int? InstallmentCount { get; set; }
        public bool IsIncoming { get; set; } = true; // True=Tahsilat (Gelen), False=Ödeme (Giden)

        // Navigation Properties
        public virtual User User { get; set; } = null!;
        public virtual Order? Order { get; set; }
    }
}
