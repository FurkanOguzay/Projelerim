using Microsoft.EntityFrameworkCore;
using UpexTech.Business.DTOs;
using UpexTech.Data;
using UpexTech.Data.Repositories;
using UpexTech.Entity;

namespace UpexTech.Business.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IRepository<Payment> _paymentRepository;
        private readonly IRepository<User> _userRepository;
        private readonly UpexTechDbContext _context;

        public PaymentService(
            IRepository<Payment> paymentRepository,
            IRepository<User> userRepository,
            UpexTechDbContext context)
        {
            _paymentRepository = paymentRepository;
            _userRepository = userRepository;
            _context = context;
        }

        public async Task<PaymentSummaryDto> GetPaymentSummaryAsync()
        {
            var payments = await _paymentRepository.Query()
                .Where(p => p.IsActive && p.Status == PaymentStatus.Completed)
                .ToListAsync();

            var incoming = payments.Where(p => p.IsIncoming);
            var outgoing = payments.Where(p => !p.IsIncoming);

            return new PaymentSummaryDto
            {
                TotalIncoming = incoming.Sum(p => p.Amount),
                IncomingCount = incoming.Count(),
                TotalOutgoing = outgoing.Sum(p => p.Amount),
                OutgoingCount = outgoing.Count(),
                NetFlow = incoming.Sum(p => p.Amount) - outgoing.Sum(p => p.Amount)
            };
        }

        public async Task<IEnumerable<PaymentListDto>> GetPaymentsAsync(PaymentFilterDto? filter = null)
        {
            var query = _paymentRepository.Query()
                .Include(p => p.User)
                .Include(p => p.Order)
                .Where(p => p.IsActive);

            // Apply filters
            if (filter != null)
            {
                if (!string.IsNullOrEmpty(filter.SearchTerm))
                {
                    var term = filter.SearchTerm.ToLower();
                    query = query.Where(p => 
                        (p.User.FirstName + " " + p.User.LastName).ToLower().Contains(term) ||
                        (p.User.CompanyName != null && p.User.CompanyName.ToLower().Contains(term)) ||
                        (p.Description != null && p.Description.ToLower().Contains(term)) ||
                        (p.ReferenceNumber != null && p.ReferenceNumber.ToLower().Contains(term)));
                }

                if (!string.IsNullOrEmpty(filter.Channel) && filter.Channel != "Tümü")
                {
                    var channel = filter.Channel == "B2C" ? PaymentChannel.B2C : PaymentChannel.B2B;
                    query = query.Where(p => p.Channel == channel);
                }

                if (!string.IsNullOrEmpty(filter.Status) && filter.Status != "Tümü")
                {
                    var status = filter.Status switch
                    {
                        "Başarılı" => PaymentStatus.Completed,
                        "Beklemede" => PaymentStatus.Pending,
                        "Başarısız" => PaymentStatus.Failed,
                        _ => PaymentStatus.Completed
                    };
                    query = query.Where(p => p.Status == status);
                }

                if (filter.StartDate.HasValue)
                {
                    query = query.Where(p => p.PaymentDate >= filter.StartDate.Value);
                }

                if (filter.EndDate.HasValue)
                {
                    query = query.Where(p => p.PaymentDate <= filter.EndDate.Value);
                }

                if (filter.IsIncoming.HasValue)
                {
                    query = query.Where(p => p.IsIncoming == filter.IsIncoming.Value);
                }
            }

            var payments = await query
                .OrderByDescending(p => p.PaymentDate)
                .Take(100)
                .ToListAsync();

            return payments.Select(p => new PaymentListDto
            {
                Id = p.Id,
                PaymentDate = p.PaymentDate,
                Channel = p.Channel == PaymentChannel.B2C ? "B2C" : "B2B",
                CustomerName = p.User.FullName,
                CompanyName = p.User.CompanyName,
                Description = p.Description ?? (p.Order != null ? $"Sipariş #{p.Order.OrderNumber} ödemesi" : "Ödeme"),
                ReferenceNumber = p.ReferenceNumber ?? (p.Order != null ? $"Ref: {p.Order.OrderNumber}" : null),
                AccountName = p.AccountName ?? GetAccountNameByMethod(p.PaymentMethod),
                PaymentMethod = GetPaymentMethodText(p.PaymentMethod),
                InstallmentCount = p.InstallmentCount,
                Amount = p.Amount,
                IsIncoming = p.IsIncoming,
                Status = GetStatusText(p.Status),
                OrderId = p.OrderId,
                OrderNumber = p.Order?.OrderNumber
            });
        }

        public async Task<PaymentListDto?> GetPaymentByIdAsync(int id)
        {
            var payment = await _paymentRepository.Query()
                .Include(p => p.User)
                .Include(p => p.Order)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (payment == null) return null;

            return new PaymentListDto
            {
                Id = payment.Id,
                PaymentDate = payment.PaymentDate,
                Channel = payment.Channel == PaymentChannel.B2C ? "B2C" : "B2B",
                CustomerName = payment.User.FullName,
                CompanyName = payment.User.CompanyName,
                Description = payment.Description ?? "",
                ReferenceNumber = payment.ReferenceNumber,
                AccountName = payment.AccountName ?? "",
                PaymentMethod = GetPaymentMethodText(payment.PaymentMethod),
                InstallmentCount = payment.InstallmentCount,
                Amount = payment.Amount,
                IsIncoming = payment.IsIncoming,
                Status = GetStatusText(payment.Status),
                OrderId = payment.OrderId,
                OrderNumber = payment.Order?.OrderNumber
            };
        }

        public async Task<int> CreateManualPaymentAsync(CreateManualPaymentDto dto)
        {
            var user = await _userRepository.GetByIdAsync(dto.UserId);
            if (user == null)
                throw new ArgumentException("Kullanıcı bulunamadı");

            var payment = new Payment
            {
                UserId = dto.UserId,
                Amount = dto.Amount,
                PaymentMethod = PaymentMethod.BankTransfer,
                Status = PaymentStatus.Completed,
                Channel = user.Role == UserRole.B2B ? PaymentChannel.B2B : PaymentChannel.B2C,
                PaymentDate = DateTime.Now,
                BankName = dto.BankName,
                AccountName = dto.BankName,
                Description = dto.Description ?? "Manuel Tahsilat - Banka Havalesi/EFT",
                ReferenceNumber = dto.ReferenceNumber ?? $"MNL-{DateTime.Now:yyyyMMddHHmmss}",
                IsIncoming = true,
                CreatedAt = DateTime.Now
            };

            await _paymentRepository.AddAsync(payment);
            return payment.Id;
        }

        public async Task<InvoiceDetailDto?> GetInvoiceDetailAsync(int orderId)
        {
            var order = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null) return null;

            var payment = await _paymentRepository.Query()
                .FirstOrDefaultAsync(p => p.OrderId == orderId && p.Status == PaymentStatus.Completed);

            var items = order.OrderItems.Select(oi => new InvoiceItemDto
            {
                ProductName = oi.Product.Name,
                Quantity = oi.Quantity,
                UnitPrice = oi.UnitPrice,
                TaxRate = 20,
                Total = oi.TotalPrice
            }).ToList();

            var subTotal = items.Sum(i => i.UnitPrice * i.Quantity);
            var taxAmount = subTotal * 0.20m;

            return new InvoiceDetailDto
            {
                InvoiceNumber = $"FAT-{order.CreatedAt.Year}/{order.Id:D3}",
                InvoiceDate = order.CreatedAt,
                IsPaid = payment != null,
                CustomerName = order.User.FullName,
                OrderNumber = $"Sipariş #{order.OrderNumber} ödemesi",
                Items = items,
                SubTotal = subTotal,
                TaxRate = 20,
                TaxAmount = taxAmount,
                GrandTotal = order.TotalAmount
            };
        }

        public Task<IEnumerable<BankAccountDto>> GetBankAccountsAsync()
        {
            var banks = new List<BankAccountDto>
            {
                new() { Name = "Garanti BBVA", Value = "garanti" },
                new() { Name = "İş Bankası", Value = "isbank" },
                new() { Name = "Yapı Kredi", Value = "yapikredi" },
                new() { Name = "Ziraat Bankası", Value = "ziraat" },
                new() { Name = "Akbank", Value = "akbank" },
                new() { Name = "QNB Finansbank", Value = "qnb" },
                new() { Name = "Denizbank", Value = "denizbank" },
                new() { Name = "Halkbank", Value = "halkbank" },
                new() { Name = "Vakıfbank", Value = "vakifbank" }
            };

            return Task.FromResult<IEnumerable<BankAccountDto>>(banks);
        }

        private static string GetPaymentMethodText(PaymentMethod method)
        {
            return method switch
            {
                PaymentMethod.CreditCard => "Kredi Kartı",
                PaymentMethod.BankTransfer => "Banka Havalesi",
                PaymentMethod.IyzicoPOS => "Iyzico POS",
                PaymentMethod.CashOnDelivery => "Kapıda Ödeme",
                _ => "Diğer"
            };
        }

        private static string GetAccountNameByMethod(PaymentMethod method)
        {
            return method switch
            {
                PaymentMethod.CreditCard => "Kredi Kartı",
                PaymentMethod.BankTransfer => "Banka Havalesi",
                PaymentMethod.IyzicoPOS => "Iyzico POS",
                _ => "Diğer"
            };
        }

        private static string GetStatusText(PaymentStatus status)
        {
            return status switch
            {
                PaymentStatus.Pending => "Beklemede",
                PaymentStatus.Completed => "Başarılı",
                PaymentStatus.Failed => "Başarısız",
                PaymentStatus.Refunded => "İade Edildi",
                _ => "Bilinmiyor"
            };
        }
    }
}
