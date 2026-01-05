namespace UpexTech.Business.DTOs
{
    public class PaymentListDto
    {
        public int Id { get; set; }
        public DateTime PaymentDate { get; set; }
        public string Channel { get; set; } = string.Empty; // B2C / B2B
        public string CustomerName { get; set; } = string.Empty;
        public string? CompanyName { get; set; }
        public string Description { get; set; } = string.Empty;
        public string? ReferenceNumber { get; set; }
        public string AccountName { get; set; } = string.Empty;
        public string PaymentMethod { get; set; } = string.Empty;
        public int? InstallmentCount { get; set; }
        public decimal Amount { get; set; }
        public bool IsIncoming { get; set; }
        public string Status { get; set; } = string.Empty;
        public int? OrderId { get; set; }
        public string? OrderNumber { get; set; }
    }

    public class PaymentSummaryDto
    {
        public decimal TotalIncoming { get; set; } // Toplam Tahsilat
        public int IncomingCount { get; set; }
        public decimal TotalOutgoing { get; set; } // Toplam Ödeme
        public int OutgoingCount { get; set; }
        public decimal NetFlow { get; set; } // Net Akış
    }

    public class PaymentFilterDto
    {
        public string? SearchTerm { get; set; }
        public string? Channel { get; set; } // B2C, B2B, Tümü
        public string? AccountName { get; set; }
        public string? Status { get; set; } // Başarılı, Beklemede, Başarısız
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool? IsIncoming { get; set; }
    }

    public class CreateManualPaymentDto
    {
        public int UserId { get; set; }
        public string BankName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string? Description { get; set; }
        public string? ReferenceNumber { get; set; }
    }

    public class InvoiceDetailDto
    {
        public string InvoiceNumber { get; set; } = string.Empty;
        public DateTime InvoiceDate { get; set; }
        public bool IsPaid { get; set; }
        
        // Company Info
        public string CompanyName { get; set; } = "UpexTech";
        public string CompanyAddress { get; set; } = "Telefon ve Telefon Parçaları\nİstanbul, Türkiye";
        
        // Customer Info
        public string CustomerName { get; set; } = string.Empty;
        public string OrderNumber { get; set; } = string.Empty;
        
        // Order Items
        public List<InvoiceItemDto> Items { get; set; } = new();
        
        // Totals
        public decimal SubTotal { get; set; }
        public decimal TaxRate { get; set; } = 20; // KDV oranı
        public decimal TaxAmount { get; set; }
        public decimal GrandTotal { get; set; }
    }

    public class InvoiceItemDto
    {
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TaxRate { get; set; }
        public decimal Total { get; set; }
    }

    public class BankAccountDto
    {
        public string Name { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
    }
}
