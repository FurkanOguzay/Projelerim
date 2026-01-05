using Microsoft.EntityFrameworkCore;
using UpexTech.Data;
using UpexTech.Data.Repositories;
using UpexTech.Entity;

namespace UpexTech.Business.Services
{
    public class AccountTransactionService : IAccountTransactionService
    {
        private readonly IRepository<AccountTransaction> _transactionRepository;
        private readonly IRepository<User> _userRepository;
        private readonly UpexTechDbContext _context;

        public AccountTransactionService(
            IRepository<AccountTransaction> transactionRepository,
            IRepository<User> userRepository,
            UpexTechDbContext context)
        {
            _transactionRepository = transactionRepository;
            _userRepository = userRepository;
            _context = context;
        }

        public async Task<IEnumerable<DealerAccountSummary>> GetAllDealerBalancesAsync()
        {
            var dealers = await _userRepository.Query()
                .Where(u => u.Role == UserRole.B2B && u.Status == UserStatus.Active)
                .ToListAsync();

            var summaries = new List<DealerAccountSummary>();

            foreach (var dealer in dealers)
            {
                var transactions = await _transactionRepository.Query()
                    .Where(t => t.UserId == dealer.Id)
                    .ToListAsync();

                var totalDebit = transactions
                    .Where(t => t.TransactionType == TransactionType.Debit)
                    .Sum(t => t.Amount);

                var totalCredit = transactions
                    .Where(t => t.TransactionType == TransactionType.Credit)
                    .Sum(t => t.Amount);

                var hasOverdue = transactions
                    .Any(t => t.TransactionType == TransactionType.Debit 
                           && t.DueDate.HasValue 
                           && t.DueDate.Value < DateTime.Now);

                var lastTransaction = transactions
                    .OrderByDescending(t => t.TransactionDate)
                    .FirstOrDefault();

                // Get dealer's orders for order count and last order date
                var dealerOrders = await _context.Orders
                    .Where(o => o.UserId == dealer.Id && o.IsActive)
                    .ToListAsync();
                
                var orderCount = dealerOrders.Count;
                var lastOrder = dealerOrders.OrderByDescending(o => o.CreatedAt).FirstOrDefault();
                
                // Get dealer's city from their user record (mock data for now)
                var cities = new[] { "İstanbul", "Ankara", "İzmir", "Bursa", "Antalya", "Adana", "Gaziantep", "Konya" };
                var districts = new[] { "Kadıköy", "Beşiktaş", "Çankaya", "Karşıyaka", "Nilüfer", "Muratpaşa", "Seyhan", "Selçuklu" };
                var cityIndex = (dealer.Id % cities.Length);
                var dealerCity = cities[cityIndex];
                var dealerDistrict = districts[cityIndex];
                
                // Calculate growth rate (mock - compare this year vs last year)
                var random = new Random(dealer.Id); // Deterministic random based on dealer ID
                var growthRate = random.Next(-10, 30);

                summaries.Add(new DealerAccountSummary
                {
                    UserId = dealer.Id,
                    CompanyName = dealer.CompanyName ?? "Belirtilmemiş",
                    FullName = dealer.FullName,
                    Email = dealer.Email,
                    Phone = dealer.Phone,
                    Tier = dealer.Tier,
                    TotalDebit = totalDebit,
                    TotalCredit = totalCredit,
                    LastTransactionDate = lastTransaction?.TransactionDate,
                    HasOverduePayments = hasOverdue,
                    CreditLimit = dealer.CreditLimit ?? 0,
                    // New Figma design fields
                    City = dealerCity,
                    District = dealerDistrict,
                    OrderCount = orderCount,
                    LastOrderDate = lastOrder?.CreatedAt,
                    GrowthRate = growthRate
                });
            }

            return summaries.OrderByDescending(s => s.Balance);
        }

        public async Task<IEnumerable<AccountTransaction>> GetDealerTransactionsAsync(int userId)
        {
            return await _transactionRepository.Query()
                .Where(t => t.UserId == userId)
                .Include(t => t.Order)
                .OrderByDescending(t => t.TransactionDate)
                .ToListAsync();
        }

        public async Task<DealerAccountSummary> GetDealerSummaryAsync(int userId)
        {
            var dealer = await _userRepository.GetByIdAsync(userId);
            if (dealer == null)
                throw new ArgumentException("Bayi bulunamadı", nameof(userId));

            var transactions = await _transactionRepository.Query()
                .Where(t => t.UserId == userId)
                .ToListAsync();

            var totalDebit = transactions
                .Where(t => t.TransactionType == TransactionType.Debit)
                .Sum(t => t.Amount);

            var totalCredit = transactions
                .Where(t => t.TransactionType == TransactionType.Credit)
                .Sum(t => t.Amount);

            var hasOverdue = transactions
                .Any(t => t.TransactionType == TransactionType.Debit 
                       && t.DueDate.HasValue 
                       && t.DueDate.Value < DateTime.Now);

            var lastTransaction = transactions
                .OrderByDescending(t => t.TransactionDate)
                .FirstOrDefault();

            return new DealerAccountSummary
            {
                UserId = dealer.Id,
                CompanyName = dealer.CompanyName ?? "Belirtilmemiş",
                FullName = dealer.FullName,
                Email = dealer.Email,
                Phone = dealer.Phone,
                Tier = dealer.Tier,
                TotalDebit = totalDebit,
                TotalCredit = totalCredit,
                LastTransactionDate = lastTransaction?.TransactionDate,
                HasOverduePayments = hasOverdue,
                CreditLimit = dealer.CreditLimit ?? 0
            };
        }

        public async Task<decimal> GetDealerBalanceAsync(int userId)
        {
            var transactions = await _transactionRepository.Query()
                .Where(t => t.UserId == userId)
                .ToListAsync();

            var totalDebit = transactions
                .Where(t => t.TransactionType == TransactionType.Debit)
                .Sum(t => t.Amount);

            var totalCredit = transactions
                .Where(t => t.TransactionType == TransactionType.Credit)
                .Sum(t => t.Amount);

            return totalDebit - totalCredit;
        }

        public async Task AddPaymentAsync(int userId, decimal amount, string? referenceNumber, string? description)
        {
            var transaction = new AccountTransaction
            {
                UserId = userId,
                TransactionType = TransactionType.Credit,
                Amount = amount,
                TransactionDate = DateTime.Now,
                ReferenceNumber = referenceNumber,
                Description = description ?? "Havale/EFT Ödemesi",
                CreatedAt = DateTime.Now
            };

            await _transactionRepository.AddAsync(transaction);
        }

        public async Task CreateOrderDebitAsync(int userId, int orderId, decimal amount, DateTime? dueDate)
        {
            var transaction = new AccountTransaction
            {
                UserId = userId,
                TransactionType = TransactionType.Debit,
                Amount = amount,
                TransactionDate = DateTime.Now,
                DueDate = dueDate ?? DateTime.Now.AddDays(30),
                OrderId = orderId,
                Description = "Sipariş Borcu",
                CreatedAt = DateTime.Now
            };

            await _transactionRepository.AddAsync(transaction);
        }
    }
}
