using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UpexTech.Admin.Models;
using UpexTech.Business.Services;
using UpexTech.Data;
using UpexTech.Entity;

namespace UpexTech.Admin.Controllers
{
    [Authorize]
    public class CariHesapController : AdminBaseController
    {
        private readonly IAccountTransactionService _transactionService;
        private readonly IUserService _userService;
        private readonly UpexTechDbContext _context;

        public CariHesapController(
            IAccountTransactionService transactionService,
            IUserService userService,
            UpexTechDbContext context)
        {
            _transactionService = transactionService;
            _userService = userService;
            _context = context;
        }

        public async Task<IActionResult> Index(string? filter = "all", string? search = null, string? taxNumber = null, string? group = null, string? balanceStatus = null)
        {
            var allDealers = await _transactionService.GetAllDealerBalancesAsync();
            var dealers = allDealers.ToList();

            // Apply search filter (company name or code)
            if (!string.IsNullOrWhiteSpace(search))
            {
                dealers = dealers.Where(d => 
                    d.CompanyName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    d.UserId.ToString().Contains(search) ||
                    $"C-{d.UserId:D5}".Contains(search, StringComparison.OrdinalIgnoreCase)
                ).ToList();
            }

            // Apply tax number filter
            if (!string.IsNullOrWhiteSpace(taxNumber))
            {
                // Mock tax number search (since we generate it from UserId)
                dealers = dealers.Where(d => 
                    (d.UserId * 123456789 % 10000000000).ToString().Contains(taxNumber)
                ).ToList();
            }

            // Apply customer group filter
            if (!string.IsNullOrWhiteSpace(group))
            {
                dealers = group switch
                {
                    "ana-bayi" => dealers.Where(d => d.Tier == CustomerTier.Platinum).ToList(),
                    "distributor" => dealers.Where(d => d.Tier == CustomerTier.Gold).ToList(),
                    "kurumsal" => dealers.Where(d => d.Tier == CustomerTier.Silver).ToList(),
                    _ => dealers
                };
            }

            // Apply balance status filter
            if (!string.IsNullOrWhiteSpace(balanceStatus))
            {
                dealers = balanceStatus switch
                {
                    "borclu" => dealers.Where(d => d.Balance > 0).ToList(),
                    "alacakli" => dealers.Where(d => d.Balance < 0).ToList(),
                    "sifir" => dealers.Where(d => d.Balance == 0).ToList(),
                    _ => dealers
                };
            }

            // Legacy filter support
            IEnumerable<DealerAccountSummary> filteredDealers = filter switch
            {
                "debtors" => dealers.Where(d => d.Balance > 0),
                "creditors" => dealers.Where(d => d.Balance < 0),
                "overdue" => dealers.Where(d => d.HasOverduePayments),
                _ => dealers
            };

            // Pass filter values to view
            ViewBag.Search = search;
            ViewBag.TaxNumber = taxNumber;
            ViewBag.Group = group;
            ViewBag.BalanceStatus = balanceStatus;

            // Calculate new KPI values for Figma design
            var totalDealerCount = allDealers.Count();
            var totalRevenue = allDealers.Sum(d => d.TotalDebit); // Toplam ciro
            var activeDealersThisMonth = allDealers.Count(d => d.LastOrderDate.HasValue && 
                d.LastOrderDate.Value >= DateTime.Now.AddDays(-30));
            var totalOrderCount = allDealers.Sum(d => d.OrderCount);
            var averageBasket = totalOrderCount > 0 ? totalRevenue / totalOrderCount : 0;

            // Get unique cities and representatives for filters
            var cities = allDealers.Select(d => d.City).Where(c => !string.IsNullOrEmpty(c)).Distinct().ToList();
            var representatives = allDealers.Select(d => d.FullName).Where(r => !string.IsNullOrEmpty(r)).Distinct().ToList();

            var viewModel = new CariHesapIndexViewModel
            {
                Dealers = filteredDealers,
                TotalReceivable = allDealers.Where(d => d.Balance > 0).Sum(d => d.Balance),
                TotalPayable = Math.Abs(allDealers.Where(d => d.Balance < 0).Sum(d => d.Balance)),
                OverdueCount = allDealers.Count(d => d.HasOverduePayments),
                Filter = filter,
                // New KPI fields
                TotalDealerCount = totalDealerCount,
                TotalRevenue = totalRevenue,
                ActiveDealersThisMonth = activeDealersThisMonth,
                AverageBasket = averageBasket,
                Cities = cities!,
                Representatives = representatives!
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateDealer(string companyName, string taxNumber, string firstName, string lastName, string email, string? phone, string tier, decimal creditLimit)
        {
            try
            {
                // Check if email already exists
                var existingUser = await _userService.GetUserByEmailAsync(email);
                if (existingUser != null)
                {
                    return Json(new { success = false, message = "Bu e-posta adresi zaten kullanılıyor." });
                }

                // Parse tier
                var customerTier = tier switch
                {
                    "Platinum" => CustomerTier.Platinum,
                    "Gold" => CustomerTier.Gold,
                    "Silver" => CustomerTier.Silver,
                    _ => CustomerTier.Standard
                };

                var user = new User
                {
                    Email = email,
                    FirstName = firstName,
                    LastName = lastName,
                    CompanyName = companyName,
                    TaxNumber = taxNumber,
                    Phone = phone,
                    Role = UserRole.B2B,
                    Status = UserStatus.Active,
                    Tier = customerTier,
                    IsActive = true,
                    CreatedAt = DateTime.Now
                };
                user.CreditLimit = creditLimit;
                var tempPassword = "temp123";

                var registeredUser = await _userService.RegisterAsync(user, tempPassword);
                
                // Activate user immediately
                var createdUser = await _userService.GetByIdAsync(registeredUser.Id);
                if (createdUser != null)
                {
                    createdUser.Status = UserStatus.Active;
                    await _userService.UpdateAsync(createdUser);
                }

                return Json(new { success = true, message = "Yeni cari kart başarıyla oluşturuldu!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Bir hata oluştu: " + ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateRiskLimit(int dealerId, decimal amount)
        {
            try
            {
                await _userService.UpdateCreditLimitAsync(dealerId, amount);
                return Json(new { success = true, message = $"Risk limiti ₺{amount:N0} olarak güncellendi!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Bir hata oluştu: " + ex.Message });
            }
        }

        public async Task<IActionResult> Statement(int id)
        {
            var dealer = await _userService.GetByIdAsync(id);
            if (dealer == null || dealer.Role != UserRole.B2B)
            {
                return NotFound();
            }

            var summary = await _transactionService.GetDealerSummaryAsync(id);
            var transactions = await _transactionService.GetDealerTransactionsAsync(id);

            // Running balance hesapla
            var orderedTransactions = transactions.OrderBy(t => t.TransactionDate).ThenBy(t => t.Id).ToList();
            decimal runningBalance = 0;
            var statementLines = new List<StatementLineItem>();

            foreach (var t in orderedTransactions)
            {
                if (t.TransactionType == TransactionType.Debit)
                {
                    runningBalance += t.Amount;
                    statementLines.Add(new StatementLineItem
                    {
                        Id = t.Id,
                        Date = t.TransactionDate,
                        TransactionType = "Borç",
                        Description = t.Description ?? "Sipariş Borcu",
                        ReferenceNumber = t.Order?.OrderNumber ?? t.ReferenceNumber,
                        Debit = t.Amount,
                        Credit = 0,
                        RunningBalance = runningBalance,
                        IsOverdue = t.DueDate.HasValue && t.DueDate.Value < DateTime.Now
                    });
                }
                else
                {
                    runningBalance -= t.Amount;
                    statementLines.Add(new StatementLineItem
                    {
                        Id = t.Id,
                        Date = t.TransactionDate,
                        TransactionType = "Alacak",
                        Description = t.Description ?? "Ödeme",
                        ReferenceNumber = t.ReferenceNumber,
                        Debit = 0,
                        Credit = t.Amount,
                        RunningBalance = runningBalance,
                        IsOverdue = false
                    });
                }
            }

            var viewModel = new StatementViewModel
            {
                Dealer = dealer,
                DealerSummary = summary,
                Transactions = statementLines.OrderByDescending(s => s.Date).ThenByDescending(s => s.Id)
            };

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> AddPayment(int id)
        {
            var dealer = await _userService.GetByIdAsync(id);
            if (dealer == null || dealer.Role != UserRole.B2B)
            {
                return NotFound();
            }

            var balance = await _transactionService.GetDealerBalanceAsync(id);

            var viewModel = new AddPaymentViewModel
            {
                UserId = id,
                DealerName = dealer.CompanyName ?? dealer.FullName,
                CurrentBalance = balance
            };

            return PartialView("_AddPaymentModal", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddPayment(AddPaymentViewModel model)
        {
            if (model.Amount <= 0)
            {
                TempData["ErrorMessage"] = "Ödeme tutarı 0'dan büyük olmalıdır.";
                return RedirectToAction("Statement", new { id = model.UserId });
            }

            await _transactionService.AddPaymentAsync(
                model.UserId,
                model.Amount,
                model.ReferenceNumber,
                model.Description ?? "Ödeme");

            TempData["SuccessMessage"] = $"{model.Amount:N2} ₺ tutarında ödeme başarıyla kaydedildi.";
            return RedirectToAction("Statement", new { id = model.UserId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddDebit(AddPaymentViewModel model)
        {
            if (model.Amount <= 0)
            {
                TempData["ErrorMessage"] = "Borç tutarı 0'dan büyük olmalıdır.";
                return RedirectToAction("Statement", new { id = model.UserId });
            }

            // Manuel borç kaydı ekle
            _context.AccountTransactions.Add(new AccountTransaction
            {
                UserId = model.UserId,
                TransactionType = TransactionType.Debit,
                Amount = model.Amount,
                Description = model.Description ?? "Manuel Borç Kaydı",
                ReferenceNumber = model.ReferenceNumber ?? $"MB-{DateTime.Now:yyMMdd}-{new Random().Next(1000, 9999)}",
                TransactionDate = DateTime.Now,
                DueDate = DateTime.Now.AddDays(30),
                OrderId = null
            });
            
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"{model.Amount:N2} ₺ tutarında borç kaydı başarıyla eklendi.";
            return RedirectToAction("Statement", new { id = model.UserId });
        }

        [HttpGet]
        public async Task<IActionResult> SeedDemoData()
        {
            // Create demo B2B dealers
            var demoUsers = new[]
            {
                new User { Email = "demo1@upexteknoloji.com.tr", PasswordHash = "demo123", FirstName = "Ahmet", LastName = "Yılmaz", CompanyName = "Upex Teknoloji A.Ş.", Phone = "+90 216 555 12 34", TaxNumber = "1234567890", Role = UserRole.B2B, Status = UserStatus.Active, Tier = CustomerTier.Platinum, IsActive = true, CreatedAt = DateTime.Now },
                new User { Email = "demo2@dijitaldunya.com.tr", PasswordHash = "demo123", FirstName = "Mehmet", LastName = "Demir", CompanyName = "Dijital Dünya Ticaret Ltd. Şti.", Phone = "+90 312 444 56 78", TaxNumber = "9876543210", Role = UserRole.B2B, Status = UserStatus.Active, Tier = CustomerTier.Gold, IsActive = true, CreatedAt = DateTime.Now },
                new User { Email = "demo3@akillitelefon.com.tr", PasswordHash = "demo123", FirstName = "Ayşe", LastName = "Kaya", CompanyName = "Akıllı Telefon Dağıtım A.Ş.", Phone = "+90 232 333 44 55", TaxNumber = "5551234567", Role = UserRole.B2B, Status = UserStatus.Active, Tier = CustomerTier.Platinum, IsActive = true, CreatedAt = DateTime.Now },
                new User { Email = "demo4@mobildunya.com.tr", PasswordHash = "demo123", FirstName = "Ali", LastName = "Çelik", CompanyName = "Mobil Dünya Elektronik San. Tic.", Phone = "+90 224 777 88 99", TaxNumber = "7778889990", Role = UserRole.B2B, Status = UserStatus.Active, Tier = CustomerTier.Silver, IsActive = true, CreatedAt = DateTime.Now },
                new User { Email = "demo5@elektronikmerkezi.com.tr", PasswordHash = "demo123", FirstName = "Fatma", LastName = "Arslan", CompanyName = "Elektronik Merkezi İth. İhr. Ltd.", Phone = "+90 242 666 77 88", TaxNumber = "3334445556", Role = UserRole.B2B, Status = UserStatus.Active, Tier = CustomerTier.Gold, IsActive = true, CreatedAt = DateTime.Now },
            };

            foreach (var user in demoUsers)
            {
                var existingUser = await _userService.GetUserByEmailAsync(user.Email);
                if (existingUser == null)
                {
                    var registeredUser = await _userService.RegisterAsync(user, user.PasswordHash);
                    
                    // Activate the user (RegisterAsync sets B2B users to Pending by default)
                    var createdUser = await _userService.GetByIdAsync(registeredUser.Id);
                    if (createdUser == null) continue;
                    createdUser.Status = UserStatus.Active;
                    await _userService.UpdateAsync(createdUser);
                    
                    // Add transactions directly using DbContext
                    var random = new Random();
                    var debitAmount = random.Next(100000, 800000);
                    var creditAmount = random.Next(50000, debitAmount - 10000);
                    
                    // Debit transaction (borç)
                    _context.AccountTransactions.Add(new AccountTransaction
                    {
                        UserId = createdUser.Id,
                        TransactionType = TransactionType.Debit,
                        Amount = debitAmount,
                        Description = "Sipariş Borcu",
                        ReferenceNumber = $"SP-{random.Next(10000, 99999)}",
                        TransactionDate = DateTime.Now.AddDays(-random.Next(5, 30)),
                        DueDate = DateTime.Now.AddDays(30),
                        OrderId = null
                    });
                    
                    // Credit transaction (ödeme)
                    _context.AccountTransactions.Add(new AccountTransaction
                    {
                        UserId = createdUser.Id,
                        TransactionType = TransactionType.Credit,
                        Amount = creditAmount,
                        Description = "Ödeme",
                        ReferenceNumber = $"OD-{random.Next(10000, 99999)}",
                        TransactionDate = DateTime.Now.AddDays(-random.Next(1, 5)),
                        OrderId = null
                    });
                    
                    await _context.SaveChangesAsync();
                }
            }

            TempData["SuccessMessage"] = "Demo bayi verileri başarıyla eklendi!";
            return RedirectToAction("Index");
        }
    }
}
