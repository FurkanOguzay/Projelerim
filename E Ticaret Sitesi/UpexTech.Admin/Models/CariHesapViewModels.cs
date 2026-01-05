using UpexTech.Business.Services;
using UpexTech.Entity;

namespace UpexTech.Admin.Models
{
    public class CariHesapIndexViewModel
    {
        public IEnumerable<DealerAccountSummary> Dealers { get; set; } = new List<DealerAccountSummary>();
        public decimal TotalReceivable { get; set; }  // Toplam Alacak (Borçlu bakiyeler)
        public decimal TotalPayable { get; set; }     // Toplam Borç (Alacaklı bakiyeler)
        public int OverdueCount { get; set; }         // Vadesi geçen sayısı
        public string? Filter { get; set; }           // all, debtors, creditors, overdue
        
        // New KPI fields for Figma design
        public decimal TotalRevenue { get; set; }     // Toplam Ciro (Yıllık)
        public int ActiveDealersThisMonth { get; set; } // Bu Ay Aktif Bayiler
        public decimal AverageBasket { get; set; }    // Ortalama Sepet Tutarı
        public int TotalDealerCount { get; set; }     // Toplam Bayi Sayısı
        
        // Filter options
        public List<string> Cities { get; set; } = new List<string>();
        public List<string> Representatives { get; set; } = new List<string>();
    }

    public class StatementViewModel
    {
        public DealerAccountSummary DealerSummary { get; set; } = null!;
        public IEnumerable<StatementLineItem> Transactions { get; set; } = new List<StatementLineItem>();
        public User Dealer { get; set; } = null!;
    }

    public class StatementLineItem
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string TransactionType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? ReferenceNumber { get; set; }
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
        public decimal RunningBalance { get; set; }
        public bool IsOverdue { get; set; }
    }

    public class AddPaymentViewModel
    {
        public int UserId { get; set; }
        public string DealerName { get; set; } = string.Empty;
        public decimal CurrentBalance { get; set; }
        public decimal Amount { get; set; }
        public string? ReferenceNumber { get; set; }
        public string? Description { get; set; }
    }
}
