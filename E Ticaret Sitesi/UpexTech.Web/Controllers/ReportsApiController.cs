using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using UpexTech.Business.Services;
using UpexTech.Entity;

namespace UpexTech.Web.Controllers
{
    /// <summary>
    /// Raporlar sayfası için API controller - B2B finansal işlemleri
    /// </summary>
    [Route("api/reports")]
    [ApiController]
    [Authorize]
    public class ReportsApiController : ControllerBase
    {
        private readonly IAccountTransactionService _transactionService;
        private readonly IUserService _userService;

        public ReportsApiController(
            IAccountTransactionService transactionService,
            IUserService userService)
        {
            _transactionService = transactionService;
            _userService = userService;
        }

        private int GetUserId() => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        /// <summary>
        /// Ekstre indir - PDF olarak cari hesap özeti
        /// </summary>
        [HttpGet("statement")]
        public async Task<IActionResult> DownloadStatement()
        {
            var userId = GetUserId();
            var user = await _userService.GetByIdAsync(userId);
            
            if (user == null || user.Role != UserRole.B2B)
            {
                return Forbid();
            }

            try
            {
                var summary = await _transactionService.GetDealerSummaryAsync(userId);
                
                // Basit bir text dosyası oluştur (gerçek uygulamada PDF library kullanılır)
                var content = $@"
===============================================
            CARİ HESAP EKSTRESİ
===============================================
Tarih: {DateTime.Now:dd/MM/yyyy HH:mm}
Bayi: {user.FirstName} {user.LastName}
===============================================

Cari Bakiye:      {summary.Balance:N2} ₺
Toplam Borç:      {summary.TotalDebit:N2} ₺
Toplam Ödenen:    {summary.TotalCredit:N2} ₺

Vadesi Geçen:     {(summary.HasOverduePayments ? "VAR" : "YOK")}
Son İşlem:        {summary.LastTransactionDate?.ToString("dd/MM/yyyy") ?? "-"}

===============================================
           UpexTech Ticaret A.Ş.
===============================================
";
                
                var bytes = System.Text.Encoding.UTF8.GetBytes(content);
                return File(bytes, "text/plain", $"ekstre_{DateTime.Now:yyyyMMdd}.txt");
            }
            catch
            {
                return NotFound(new { message = "Henüz işlem kaydınız bulunmuyor." });
            }
        }

        /// <summary>
        /// Ödeme geçmişi - Son işlemler listesi
        /// </summary>
        [HttpGet("payment-history")]
        public async Task<IActionResult> GetPaymentHistory()
        {
            var userId = GetUserId();
            var user = await _userService.GetByIdAsync(userId);
            
            if (user == null || user.Role != UserRole.B2B)
            {
                return Forbid();
            }

            try
            {
                var transactions = await _transactionService.GetDealerTransactionsAsync(userId);
                
                var result = transactions.Select(t => new
                {
                    date = t.TransactionDate,
                    description = t.Description ?? (t.TransactionType == TransactionType.Debit ? "Borç" : "Ödeme"),
                    amount = t.TransactionType == TransactionType.Debit ? t.Amount : -t.Amount,
                    status = "Completed"
                }).OrderByDescending(t => t.date).Take(20).ToList();
                
                return Ok(result);
            }
            catch
            {
                return Ok(new object[] { });
            }
        }

        /// <summary>
        /// Ödeme yap - Ödeme talebi oluştur
        /// </summary>
        [HttpPost("make-payment")]
        public async Task<IActionResult> MakePayment([FromBody] PaymentRequest request)
        {
            var userId = GetUserId();
            var user = await _userService.GetByIdAsync(userId);
            
            if (user == null || user.Role != UserRole.B2B)
            {
                return Forbid();
            }

            if (request.Amount <= 0)
            {
                return BadRequest(new { message = "Geçersiz tutar." });
            }

            try
            {
                // Ödeme işlemini kaydet
                var referenceNumber = $"PAY-{DateTime.Now:yyyyMMddHHmmss}";
                var description = $"Online Ödeme - {request.Method}" + 
                    (string.IsNullOrEmpty(request.Note) ? "" : $" - {request.Note}");
                    
                await _transactionService.AddPaymentAsync(userId, request.Amount, referenceNumber, description);

                return Ok(new { message = "Ödeme başarıyla kaydedildi." });
            }
            catch
            {
                return StatusCode(500, new { message = "Ödeme işlemi sırasında bir hata oluştu." });
            }
        }
    }

    public class PaymentRequest
    {
        public decimal Amount { get; set; }
        public string Method { get; set; } = "";
        public string? Note { get; set; }
    }
}
