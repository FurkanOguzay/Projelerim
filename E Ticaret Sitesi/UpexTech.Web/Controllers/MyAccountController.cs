using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using UpexTech.Business.Services;
using UpexTech.Data.Repositories;
using UpexTech.Entity;
using UpexTech.Web.Models;

namespace UpexTech.Web.Controllers
{
    [Authorize]
    public class MyAccountController : Controller
    {
        private readonly IUserService _userService;
        private readonly IRepository<Order> _orderRepository;
        private readonly IAccountTransactionService _transactionService;
        private readonly IReviewService _reviewService;
        private readonly IBrowsingHistoryService _browsingHistoryService;
        private readonly ISavedCardService _savedCardService;

        public MyAccountController(
            IUserService userService, 
            IRepository<Order> orderRepository,
            IAccountTransactionService transactionService,
            IReviewService reviewService,
            IBrowsingHistoryService browsingHistoryService,
            ISavedCardService savedCardService)
        {
            _userService = userService;
            _orderRepository = orderRepository;
            _transactionService = transactionService;
            _reviewService = reviewService;
            _browsingHistoryService = browsingHistoryService;
            _savedCardService = savedCardService;
        }

        private int GetUserId() => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        // Varsayılan sayfa - Tüm Siparişlerim
        public async Task<IActionResult> Index()
        {
            return await Orders();
        }

        #region SİPARİŞLER

        // Tüm Siparişlerim
        public async Task<IActionResult> Orders()
        {
            var userId = GetUserId();
            var orders = await _orderRepository.Query()
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
            
            ViewBag.ActiveMenu = "orders";
            return View("Index", new MyAccountViewModel
            {
                Section = "Orders",
                Orders = orders
            });
        }

        // Bekleyen Siparişler
        public async Task<IActionResult> PendingOrders()
        {
            var userId = GetUserId();
            var orders = await _orderRepository.Query()
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Where(o => o.UserId == userId && 
                     (o.Status == OrderStatus.Pending || o.Status == OrderStatus.Confirmed))
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
            
            ViewBag.ActiveMenu = "pending-orders";
            return View("Index", new MyAccountViewModel
            {
                Section = "PendingOrders",
                Orders = orders
            });
        }

        // Kargodaki Siparişler
        public async Task<IActionResult> ShippedOrders()
        {
            var userId = GetUserId();
            var orders = await _orderRepository.Query()
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Where(o => o.UserId == userId && o.Status == OrderStatus.Shipped)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
            
            ViewBag.ActiveMenu = "shipped-orders";
            return View("Index", new MyAccountViewModel
            {
                Section = "ShippedOrders",
                Orders = orders
            });
        }

        // Teslim Edilen Siparişler
        public async Task<IActionResult> DeliveredOrders()
        {
            var userId = GetUserId();
            var orders = await _orderRepository.Query()
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Where(o => o.UserId == userId && o.Status == OrderStatus.Delivered)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
            
            ViewBag.ActiveMenu = "delivered-orders";
            return View("Index", new MyAccountViewModel
            {
                Section = "DeliveredOrders",
                Orders = orders
            });
        }

        // İptal/İade Edilen Siparişler
        public async Task<IActionResult> CancelledOrders()
        {
            var userId = GetUserId();
            var orders = await _orderRepository.Query()
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Where(o => o.UserId == userId && 
                     (o.Status == OrderStatus.Cancelled || o.Status == OrderStatus.Returned))
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
            
            ViewBag.ActiveMenu = "cancelled-orders";
            return View("Index", new MyAccountViewModel
            {
                Section = "CancelledOrders",
                Orders = orders
            });
        }

        #endregion

        #region HESABIM

        // Kullanıcı Bilgilerim
        public async Task<IActionResult> Profile()
        {
            var userId = GetUserId();
            var user = await _userService.GetByIdAsync(userId);
            
            ViewBag.ActiveMenu = "profile";
            return View("Index", new MyAccountViewModel
            {
                Section = "Profile",
                User = user
            });
        }

        // Kullanıcı Bilgilerini Güncelle
        [HttpPost]
        public async Task<IActionResult> UpdateProfile(User model)
        {
            var userId = GetUserId();
            var user = await _userService.GetByIdAsync(userId);
            
            if (user == null)
            {
                return NotFound();
            }

            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.Phone = model.Phone;

            await _userService.UpdateAsync(user);
            TempData["Success"] = "Bilgileriniz başarıyla güncellendi.";
            
            return RedirectToAction("Profile");
        }

        // Adres Bilgilerim
        public async Task<IActionResult> Addresses()
        {
            var userId = GetUserId();
            var user = await _userService.GetByIdAsync(userId);
            
            ViewBag.ActiveMenu = "addresses";
            return View("Index", new MyAccountViewModel
            {
                Section = "Addresses",
                User = user
            });
        }

        // Adres Güncelle
        [HttpPost]
        public async Task<IActionResult> UpdateAddress(string address)
        {
            var userId = GetUserId();
            var user = await _userService.GetByIdAsync(userId);
            
            if (user == null)
            {
                return NotFound();
            }

            user.Address = address;
            await _userService.UpdateAsync(user);
            TempData["Success"] = "Adres bilgileriniz başarıyla güncellendi.";
            
            return RedirectToAction("Addresses");
        }

        // Şifre Değiştir
        public IActionResult ChangePassword()
        {
            ViewBag.ActiveMenu = "change-password";
            return View("Index", new MyAccountViewModel
            {
                Section = "ChangePassword"
            });
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(string currentPassword, string newPassword, string confirmPassword)
        {
            if (newPassword != confirmPassword)
            {
                TempData["Error"] = "Yeni şifreler eşleşmiyor.";
                return RedirectToAction("ChangePassword");
            }

            var userId = GetUserId();
            var result = await _userService.ChangePasswordAsync(userId, currentPassword, newPassword);
            
            if (result.Success)
            {
                TempData["Success"] = "Şifreniz başarıyla değiştirildi.";
            }
            else
            {
                TempData["Error"] = result.ErrorMessage;
            }
            
            return RedirectToAction("ChangePassword");
        }

        // Favorilerim
        public async Task<IActionResult> Favorites()
        {
            var userId = GetUserId();
            // FavoriteService kullanılabilir, şimdilik basit tutuyoruz
            
            ViewBag.ActiveMenu = "favorites";
            return View("Index", new MyAccountViewModel
            {
                Section = "Favorites"
            });
        }

        // Değerlendirmelerim
        public async Task<IActionResult> Reviews()
        {
            var userId = GetUserId();
            
            // Gerçek review verilerini çek
            var userReviews = await _reviewService.GetUserReviewsAsync(userId);
            var reviews = userReviews.Select(r => new ReviewViewModel
            {
                Id = r.Id,
                ProductId = r.ProductId,
                ProductName = r.Product?.Name ?? "Bilinmeyen Ürün",
                ProductImage = r.Product?.Image?.StartsWith("http") == true 
                    ? r.Product.Image 
                    : $"/images/{r.Product?.Image ?? "placeholder.png"}",
                Rating = r.Rating,
                Comment = r.Comment,
                CreatedAt = r.CreatedAt
            }).ToList();
            
            ViewBag.ActiveMenu = "reviews";
            return View("Index", new MyAccountViewModel
            {
                Section = "Reviews",
                Reviews = reviews
            });
        }

        // Değerlendirme Güncelle
        [HttpPost]
        public async Task<IActionResult> UpdateReview([FromBody] UpdateReviewRequest request)
        {
            var userId = GetUserId();
            
            // Review'in bu kullanıcıya ait olduğunu kontrol et
            var review = await _reviewService.GetReviewByIdAsync(request.ReviewId);
            if (review == null || review.UserId != userId)
            {
                return Json(new { success = false, message = "Değerlendirme bulunamadı" });
            }
            
            // Validation
            if (request.Rating < 1 || request.Rating > 5)
            {
                return Json(new { success = false, message = "Puan 1-5 arasında olmalıdır" });
            }
            
            if (string.IsNullOrWhiteSpace(request.Comment) || request.Comment.Length < 10)
            {
                return Json(new { success = false, message = "Yorum en az 10 karakter olmalıdır" });
            }
            
            review.Rating = request.Rating;
            review.Comment = request.Comment;
            
            var success = await _reviewService.UpdateReviewAsync(review);
            
            return Json(new { 
                success, 
                message = success ? "Değerlendirmeniz güncellendi" : "Güncelleme başarısız" 
            });
        }

        // Değerlendirme Sil
        [HttpPost]
        public async Task<IActionResult> DeleteReview([FromBody] DeleteReviewRequest request)
        {
            var userId = GetUserId();
            
            // Review'in bu kullanıcıya ait olduğunu kontrol et
            var review = await _reviewService.GetReviewByIdAsync(request.ReviewId);
            if (review == null || review.UserId != userId)
            {
                return Json(new { success = false, message = "Değerlendirme bulunamadı" });
            }
            
            var success = await _reviewService.DeleteReviewAsync(request.ReviewId);
            
            return Json(new { 
                success, 
                message = success ? "Değerlendirmeniz silindi" : "Silme başarısız" 
            });
        }

        // Satıcı Mesajlarım
        public async Task<IActionResult> Messages()
        {
            var userId = GetUserId();
            
            // Demo veriler
            var messages = new List<MessageViewModel>
            {
                new MessageViewModel
                {
                    Id = 1,
                    SellerName = "TechStore",
                    Subject = "Sipariş Hakkında",
                    LastMessage = "Ürününüz bugün kargoya verilmiştir.",
                    LastMessageDate = DateTime.Now.AddHours(-2),
                    IsRead = false
                },
                new MessageViewModel
                {
                    Id = 2,
                    SellerName = "ElektronikMarket",
                    Subject = "İade Talebi",
                    LastMessage = "İade talebiniz onaylandı.",
                    LastMessageDate = DateTime.Now.AddDays(-3),
                    IsRead = true
                }
            };
            
            ViewBag.ActiveMenu = "messages";
            return View("Index", new MyAccountViewModel
            {
                Section = "Messages",
                Messages = messages
            });
        }

        // Tekrar Satın Al
        public async Task<IActionResult> Reorder()
        {
            var userId = GetUserId();
            
            // Gerçek sipariş geçmişinden ürünleri al
            var orders = await _orderRepository.Query()
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Where(o => o.UserId == userId && o.Status != OrderStatus.Cancelled)
                .ToListAsync();
            
            // Sipariş kalemlerinden ürünleri grupla
            var reorderProducts = orders
                .SelectMany(o => o.OrderItems.Select(oi => new { 
                    oi.Product, 
                    oi.Quantity, 
                    OrderDate = o.CreatedAt 
                }))
                .Where(x => x.Product != null && x.Product.IsActive)
                .GroupBy(x => x.Product!.Id)
                .Select(g => new ReorderProductViewModel
                {
                    ProductId = g.Key,
                    ProductName = g.First().Product!.Name,
                    ProductImage = g.First().Product!.Image?.StartsWith("http") == true 
                        ? g.First().Product!.Image 
                        : $"/images/{g.First().Product!.Image ?? "placeholder.png"}",
                    Price = g.First().Product!.PriceB2C,
                    LastPurchaseDate = g.Max(x => x.OrderDate),
                    PurchaseCount = g.Sum(x => x.Quantity)
                })
                .OrderByDescending(p => p.LastPurchaseDate)
                .ToList();
            
            ViewBag.ActiveMenu = "reorder";
            return View("Index", new MyAccountViewModel
            {
                Section = "Reorder",
                ReorderProducts = reorderProducts
            });
        }

        // Önceden Gezdiklerim
        public async Task<IActionResult> BrowsingHistory()
        {
            var userId = GetUserId();
            
            // Gerçek tarama geçmişinden verileri al
            var browsingHistory = await _browsingHistoryService.GetUserBrowsingHistoryAsync(userId, 20);
            var history = browsingHistory.Select(bh => new BrowsingHistoryViewModel
            {
                ProductId = bh.ProductId,
                ProductName = bh.Product?.Name ?? "Bilinmeyen Ürün",
                ProductImage = bh.Product?.Image?.StartsWith("http") == true 
                    ? bh.Product.Image 
                    : $"/images/{bh.Product?.Image ?? "placeholder.png"}",
                Price = bh.Product?.PriceB2C ?? 0,
                ViewedAt = bh.ViewedAt
            }).ToList();
            
            ViewBag.ActiveMenu = "browsed";
            return View("Index", new MyAccountViewModel
            {
                Section = "BrowsingHistory",
                BrowsingHistory = history
            });
        }

        // Geçmişi Temizle
        [HttpPost]
        public async Task<IActionResult> ClearBrowsingHistory()
        {
            var userId = GetUserId();
            await _browsingHistoryService.ClearHistoryAsync(userId);
            return Json(new { success = true, message = "Tarama geçmişi temizlendi" });
        }

        // Kayıtlı Kartlarım
        public async Task<IActionResult> SavedCards()
        {
            var userId = GetUserId();
            
            // Gerçek veritabanından kartları çek
            var userCards = await _savedCardService.GetUserCardsAsync(userId);
            var cards = userCards.Select(c => new SavedCardViewModel
            {
                Id = c.Id,
                CardNumber = c.CardNumber,
                CardHolderName = c.CardHolderName,
                ExpiryDate = c.ExpiryDate,
                CardType = c.CardType,
                IsDefault = c.IsDefault
            }).ToList();
            
            ViewBag.ActiveMenu = "cards";
            return View("Index", new MyAccountViewModel
            {
                Section = "SavedCards",
                SavedCards = cards
            });
        }

        // Yeni Kart Ekle
        [HttpPost]
        public async Task<IActionResult> AddCard([FromBody] AddCardRequest request)
        {
            var userId = GetUserId();
            
            // Kart numarası doğrulama (16 hane)
            var cardNumber = request.CardNumber?.Replace(" ", "").Replace("-", "");
            if (string.IsNullOrEmpty(cardNumber) || cardNumber.Length != 16 || !cardNumber.All(char.IsDigit))
            {
                return Json(new { success = false, message = "Geçersiz kart numarası. 16 haneli bir kart numarası giriniz." });
            }
            
            // Kart sahibi adı
            if (string.IsNullOrWhiteSpace(request.CardHolderName) || request.CardHolderName.Length < 3)
            {
                return Json(new { success = false, message = "Kart üzerindeki isim en az 3 karakter olmalıdır." });
            }
            
            // Son kullanma tarihi doğrulama
            if (string.IsNullOrEmpty(request.ExpiryMonth) || string.IsNullOrEmpty(request.ExpiryYear))
            {
                return Json(new { success = false, message = "Son kullanma tarihi seçiniz." });
            }
            
            // Kart tipini belirle (basit kontrol)
            var cardType = "Visa";
            if (cardNumber.StartsWith("5")) cardType = "MasterCard";
            else if (cardNumber.StartsWith("9")) cardType = "Troy";
            
            var card = await _savedCardService.AddCardAsync(
                userId, 
                request.CardNumber!, 
                request.CardHolderName!, 
                request.ExpiryMonth!, 
                request.ExpiryYear!, 
                cardType);
            
            return Json(new { 
                success = true, 
                message = "Kart başarıyla eklendi",
                card = new {
                    id = card.Id,
                    cardNumber = card.CardNumber,
                    cardHolderName = card.CardHolderName,
                    expiryDate = card.ExpiryDate,
                    cardType = card.CardType,
                    isDefault = card.IsDefault
                }
            });
        }

        // Kartı Sil
        [HttpPost]
        public async Task<IActionResult> DeleteCard([FromBody] DeleteCardRequest request)
        {
            var userId = GetUserId();
            var success = await _savedCardService.DeleteCardAsync(request.CardId, userId);
            
            return Json(new { 
                success, 
                message = success ? "Kart silindi" : "Kart silinemedi" 
            });
        }

        // Varsayılan Kart Yap
        [HttpPost]
        public async Task<IActionResult> SetDefaultCard([FromBody] SetDefaultCardRequest request)
        {
            var userId = GetUserId();
            var success = await _savedCardService.SetDefaultCardAsync(request.CardId, userId);
            
            return Json(new { 
                success, 
                message = success ? "Varsayılan kart güncellendi" : "İşlem başarısız" 
            });
        }

        // Duyuru Tercihlerim
        public async Task<IActionResult> NotificationPreferences()
        {
            var userId = GetUserId();
            
            // Demo veriler - gerçek uygulamada kullanıcı tercihleri veritabanından çekilir
            var preferences = new NotificationPreferencesViewModel
            {
                EmailNotifications = true,
                SmsNotifications = false,
                PushNotifications = true,
                OrderUpdates = true,
                PromotionalEmails = true,
                PriceAlerts = false,
                NewsletterSubscription = true
            };
            
            ViewBag.ActiveMenu = "notifications";
            return View("Index", new MyAccountViewModel
            {
                Section = "NotificationPreferences",
                NotificationPreferences = preferences
            });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateNotificationPreferences(NotificationPreferencesViewModel model)
        {
            var userId = GetUserId();
            // Gerçek uygulamada veritabanına kaydedilir
            
            TempData["Success"] = "Duyuru tercihleriniz başarıyla güncellendi.";
            return RedirectToAction("NotificationPreferences");
        }

        #endregion

        #region RAPORLARIM

        // Raporlarım - B2B/B2C için farklı içerik
        public async Task<IActionResult> Reports()
        {
            var userId = GetUserId();
            var user = await _userService.GetByIdAsync(userId);
            var userRole = user?.Role ?? UserRole.B2C;
            
            var orders = (await _orderRepository.FindAsync(o => o.UserId == userId && o.IsActive))
                .OrderByDescending(o => o.CreatedAt)
                .ToList();
            
            var viewModel = new ReportsViewModel
            {
                UserRole = userRole,
                LatestOrder = orders.FirstOrDefault(),
                OrderSummary = new OrderStatusSummary
                {
                    PendingCount = orders.Count(o => o.Status == OrderStatus.Pending || o.Status == OrderStatus.Confirmed),
                    ShippedCount = orders.Count(o => o.Status == OrderStatus.Shipped),
                    DeliveredCount = orders.Count(o => o.Status == OrderStatus.Delivered),
                    CancelledCount = orders.Count(o => o.Status == OrderStatus.Cancelled || o.Status == OrderStatus.Returned)
                }
            };
            
            if (userRole == UserRole.B2B)
            {
                // B2B (Bayi): Finansal veriler
                try
                {
                    var summary = await _transactionService.GetDealerSummaryAsync(userId);
                    viewModel.Balance = summary.Balance;
                    viewModel.TotalDebit = summary.TotalDebit;
                    viewModel.TotalCredit = summary.TotalCredit;
                    viewModel.HasOverduePayments = summary.HasOverduePayments;
                    viewModel.LastTransactionDate = summary.LastTransactionDate;
                    
                    // Yeni Figma Dashboard Verileri
                    var thisMonth = DateTime.Now.Month;
                    var thisYear = DateTime.Now.Year;
                    viewModel.ThisMonthPurchase = orders
                        .Where(o => o.CreatedAt.Month == thisMonth && o.CreatedAt.Year == thisYear && o.Status != OrderStatus.Cancelled)
                        .Sum(o => o.TotalAmount);
                    
                    var pendingReturns = orders.Where(o => o.Status == OrderStatus.Returned).ToList();
                    viewModel.PendingReturns = pendingReturns.Sum(o => o.TotalAmount);
                    viewModel.PendingReturnCount = pendingReturns.Count;
                    
                    viewModel.DiscountRate = user?.Tier switch
                    {
                        CustomerTier.Platinum => 15.0m,
                        CustomerTier.Gold => 12.5m,
                        CustomerTier.Silver => 10.0m,
                        _ => 5.0m
                    };
                    viewModel.DealerTier = user?.Tier.ToString() ?? "Standard";
                    
                    // Son 6 ay trend
                    var monthlyData = new List<MonthlySpendingData>();
                    for (int i = 5; i >= 0; i--)
                    {
                        var date = DateTime.Now.AddMonths(-i);
                        var monthOrders = orders.Where(o => o.CreatedAt.Month == date.Month && o.CreatedAt.Year == date.Year && o.Status != OrderStatus.Cancelled);
                        monthlyData.Add(new MonthlySpendingData { Month = date.ToString("MMM"), Amount = monthOrders.Sum(o => o.TotalAmount) });
                    }
                    viewModel.MonthlyTrend = monthlyData;
                    
                    // Kategori dağılımı (demo)
                    viewModel.CategoryDistribution = new List<CategorySpendingData>
                    {
                        new() { CategoryName = "Aksesuar", Amount = 45, Color = "#3B82F6" },
                        new() { CategoryName = "Kılıf", Amount = 30, Color = "#10B981" },
                        new() { CategoryName = "Şarj", Amount = 25, Color = "#8B5CF6" }
                    };
                    
                    // Son hareketler
                    var transactions = await _transactionService.GetDealerTransactionsAsync(userId);
                    decimal runningBalance = 0;
                    var txList = new List<TransactionLineItem>();
                    foreach (var tx in transactions.OrderBy(t => t.TransactionDate).Take(10))
                    {
                        if (tx.TransactionType == TransactionType.Debit) runningBalance += tx.Amount;
                        else runningBalance -= tx.Amount;
                        txList.Add(new TransactionLineItem
                        {
                            Date = tx.TransactionDate,
                            TransactionNo = tx.ReferenceNumber ?? $"TRX-{tx.Id}",
                            Description = tx.Description ?? (tx.TransactionType == TransactionType.Debit ? "Sipariş" : "Ödeme"),
                            Debit = tx.TransactionType == TransactionType.Debit ? tx.Amount : 0,
                            Credit = tx.TransactionType == TransactionType.Credit ? tx.Amount : 0,
                            RunningBalance = runningBalance
                        });
                    }
                    viewModel.RecentTransactions = txList.OrderByDescending(t => t.Date).ToList();
                }
                catch
                {
                    // Henüz işlem yoksa varsayılan değerler kullanılır
                }
            }
            else
            {
                // B2C (Müşteri): Alışveriş özeti
                var thisYear = DateTime.Now.Year;
                viewModel.TotalSpentThisYear = orders
                    .Where(o => o.CreatedAt.Year == thisYear && o.Status != OrderStatus.Cancelled)
                    .Sum(o => o.TotalAmount);
                    
                viewModel.TotalSpentAllTime = orders
                    .Where(o => o.Status != OrderStatus.Cancelled)
                    .Sum(o => o.TotalAmount);
                    
                viewModel.TotalOrderCount = orders.Count(o => o.Status != OrderStatus.Cancelled);
                
                // Aktif sipariş sayısı (Pending, Confirmed, Shipped durumundakiler)
                viewModel.ActiveOrderCount = orders.Count(o => 
                    o.Status == OrderStatus.Pending || 
                    o.Status == OrderStatus.Confirmed ||
                    o.Status == OrderStatus.Shipped);
                
                // Kupon sayısı (demo veriler - gerçek uygulamada veritabanından çekilir)
                viewModel.ActiveCouponCount = 5; // Demo: 5 aktif kupon
                viewModel.TotalCouponCount = 8; // Demo: toplam 8 kupon
                
                // Son sipariş detayları (Timeline için)
                var latestOrder = orders.FirstOrDefault(o => o.Status != OrderStatus.Cancelled);
                if (latestOrder != null)
                {
                    viewModel.LatestOrderInfo = new LatestOrderDetails
                    {
                        OrderId = latestOrder.Id,
                        OrderNumber = latestOrder.Id.ToString(),
                        OrderDate = latestOrder.CreatedAt,
                        Status = latestOrder.Status,
                        EstimatedDeliveryDate = latestOrder.Status switch
                        {
                            OrderStatus.Pending => latestOrder.CreatedAt.AddDays(5),
                            OrderStatus.Confirmed => latestOrder.CreatedAt.AddDays(4),
                            OrderStatus.Shipped => latestOrder.CreatedAt.AddDays(2),
                            _ => null
                        },
                        EstimatedDeliveryTime = "14:00"
                    };
                }
                
                // Favori kategori - demo için şimdilik sabit
                viewModel.FavoriteCategory = orders.Any() ? "Aksesuar" : "Henüz alışveriş yok";
                
                // Puan hesaplama (her 10 TL = 1 puan)
                viewModel.TotalPoints = (int)(viewModel.TotalSpentAllTime / 10);
                
                // Tekrar satın al önerileri
                viewModel.SuggestedProducts = new List<ReorderProductViewModel>
                {
                    new ReorderProductViewModel
                    {
                        ProductId = 1,
                        ProductName = "Kablosuz Kulaklık Pro",
                        ProductImage = "/images/phone3.png",
                        Price = 499.90m,
                        LastPurchaseDate = DateTime.Now.AddMonths(-1),
                        PurchaseCount = 2
                    },
                    new ReorderProductViewModel
                    {
                        ProductId = 2,
                        ProductName = "USB-C Şarj Kablosu",
                        ProductImage = "/images/phone4.png",
                        Price = 89.90m,
                        LastPurchaseDate = DateTime.Now.AddMonths(-2),
                        PurchaseCount = 5
                    }
                };
            }
            
            ViewBag.ActiveMenu = "reports";
            return View("Index", new MyAccountViewModel
            {
                Section = "Reports",
                ReportsData = viewModel
            });
        }

        #endregion
    }
}
