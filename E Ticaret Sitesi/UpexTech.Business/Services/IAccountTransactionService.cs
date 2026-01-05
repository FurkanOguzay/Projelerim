using UpexTech.Entity;

namespace UpexTech.Business.Services
{
    public class DealerAccountSummary
    {
        public int UserId { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public CustomerTier Tier { get; set; }
        public decimal TotalDebit { get; set; }     // Toplam Borç
        public decimal TotalCredit { get; set; }    // Toplam Alacak
        public decimal Balance => TotalDebit - TotalCredit;  // Bakiye (+ borçlu, - alacaklı)
        public DateTime? LastTransactionDate { get; set; }
        public bool HasOverduePayments { get; set; }
        public decimal CreditLimit { get; set; }    // Risk Limiti
        
        // New fields for Figma design
        public string? City { get; set; }           // Şehir
        public string? District { get; set; }       // İlçe
        public int OrderCount { get; set; }         // Sipariş Adedi
        public DateTime? LastOrderDate { get; set; } // Son Sipariş Tarihi
        public decimal GrowthRate { get; set; }     // Büyüme Oranı (%)
    }


    public interface IAccountTransactionService
    {
        Task<IEnumerable<DealerAccountSummary>> GetAllDealerBalancesAsync();
        Task<IEnumerable<AccountTransaction>> GetDealerTransactionsAsync(int userId);
        Task<DealerAccountSummary> GetDealerSummaryAsync(int userId);
        Task<decimal> GetDealerBalanceAsync(int userId);
        Task AddPaymentAsync(int userId, decimal amount, string? referenceNumber, string? description);
        Task CreateOrderDebitAsync(int userId, int orderId, decimal amount, DateTime? dueDate);
    }
}
