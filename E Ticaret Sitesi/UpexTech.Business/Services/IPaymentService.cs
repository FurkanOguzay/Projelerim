using UpexTech.Business.DTOs;

namespace UpexTech.Business.Services
{
    public interface IPaymentService
    {
        Task<PaymentSummaryDto> GetPaymentSummaryAsync();
        Task<IEnumerable<PaymentListDto>> GetPaymentsAsync(PaymentFilterDto? filter = null);
        Task<PaymentListDto?> GetPaymentByIdAsync(int id);
        Task<int> CreateManualPaymentAsync(CreateManualPaymentDto dto);
        Task<InvoiceDetailDto?> GetInvoiceDetailAsync(int orderId);
        Task<IEnumerable<BankAccountDto>> GetBankAccountsAsync();
    }
}
