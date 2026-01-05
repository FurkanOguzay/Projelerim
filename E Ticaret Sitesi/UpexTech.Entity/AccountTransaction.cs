namespace UpexTech.Entity
{
    public enum TransactionType
    {
        Debit = 1,      // Borç (Sipariş)
        Credit = 2      // Alacak (Ödeme/Havale)
    }

    public class AccountTransaction : BaseEntity
    {
        public int UserId { get; set; }
        public TransactionType TransactionType { get; set; }
        public decimal Amount { get; set; }
        public DateTime TransactionDate { get; set; } = DateTime.Now;
        public DateTime? DueDate { get; set; }
        public string? Description { get; set; }
        public string? ReferenceNumber { get; set; }
        public int? OrderId { get; set; }

        // Navigation Properties
        public virtual User User { get; set; } = null!;
        public virtual Order? Order { get; set; }
    }
}
