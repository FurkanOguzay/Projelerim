using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UpexTech.Business.DTOs;
using UpexTech.Business.Services;

namespace UpexTech.Admin.Controllers
{
    [Authorize]
    public class FinanceController : AdminBaseController
    {
        private readonly IPaymentService _paymentService;

        public FinanceController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        public IActionResult Index()
        {
            // Summary Cards Data
            ViewBag.TotalDeposit = 1450000m;
            ViewBag.PosBalance = 125000m;
            ViewBag.TodayCollection = 45000m;

            // Bank Accounts Data
            ViewBag.BankAccounts = new[]
            {
                new { 
                    Name = "Garanti BBVA", 
                    IBAN = "****6789", 
                    Balance = 850000m, 
                    LastActivity = "Bugün 14:00",
                    Icon = "fa-building-columns"
                },
                new { 
                    Name = "İş Bankası", 
                    IBAN = "****3421", 
                    Balance = 450000m, 
                    LastActivity = "Dün 16:45",
                    Icon = "fa-building-columns"
                },
                new { 
                    Name = "Yapı Kredi", 
                    IBAN = "****8912", 
                    Balance = 150000m, 
                    LastActivity = "Bugün 11:20",
                    Icon = "fa-building-columns"
                }
            };

            // Virtual POS Data
            ViewBag.VirtualPosIntegrations = new[]
            {
                new {
                    Name = "Iyzico",
                    IsActive = true,
                    PendingAmount = 12500m,
                    PendingDate = "Yarın",
                    BlockedAmount = 85000m,
                    BlockedValor = "21 Gün Valör"
                },
                new {
                    Name = "PayTR",
                    IsActive = true,
                    PendingAmount = 8900m,
                    PendingDate = "2 Gün Sonra",
                    BlockedAmount = 32000m,
                    BlockedValor = "14 Gün Valör"
                },
                new {
                    Name = "Stripe",
                    IsActive = true,
                    PendingAmount = 0m,
                    PendingDate = "-",
                    BlockedAmount = 8000m,
                    BlockedValor = "7 Gün Valör"
                }
            };

            return View();
        }

        // Ödemeler & Tahsilatlar - Main Page
        public async Task<IActionResult> PaymentsAndCollections()
        {
            var summary = await _paymentService.GetPaymentSummaryAsync();
            var payments = await _paymentService.GetPaymentsAsync();
            var banks = await _paymentService.GetBankAccountsAsync();

            ViewBag.Summary = summary;
            ViewBag.Payments = payments;
            ViewBag.Banks = banks;

            return View();
        }

        // AJAX: Get Payments with Filters
        [HttpGet]
        public async Task<IActionResult> GetPayments(
            string? searchTerm, 
            string? channel, 
            string? status,
            DateTime? startDate,
            DateTime? endDate)
        {
            var filter = new PaymentFilterDto
            {
                SearchTerm = searchTerm,
                Channel = channel,
                Status = status,
                StartDate = startDate,
                EndDate = endDate
            };

            var payments = await _paymentService.GetPaymentsAsync(filter);
            return Json(new { success = true, data = payments });
        }

        // AJAX: Create Manual Payment (Tahsilat Ekle)
        [HttpPost]
        public async Task<IActionResult> CreateManualPayment([FromBody] CreateManualPaymentDto dto)
        {
            try
            {
                if (dto.Amount <= 0)
                    return Json(new { success = false, message = "Tutar 0'dan büyük olmalıdır." });

                if (string.IsNullOrEmpty(dto.BankName))
                    return Json(new { success = false, message = "Banka seçimi zorunludur." });

                var paymentId = await _paymentService.CreateManualPaymentAsync(dto);
                return Json(new { success = true, message = "Tahsilat başarıyla kaydedildi.", paymentId });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // AJAX: Get Invoice Detail
        [HttpGet]
        public async Task<IActionResult> GetInvoiceDetail(int orderId)
        {
            var invoice = await _paymentService.GetInvoiceDetailAsync(orderId);
            if (invoice == null)
                return Json(new { success = false, message = "Fatura bulunamadı." });

            return Json(new { success = true, data = invoice });
        }

        // AJAX: Get Summary Stats
        [HttpGet]
        public async Task<IActionResult> GetPaymentSummary()
        {
            var summary = await _paymentService.GetPaymentSummaryAsync();
            return Json(new { success = true, data = summary });
        }
    }
}
