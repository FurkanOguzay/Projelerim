using UpexTech.Entity;
using System.Text.Json.Serialization;

namespace UpexTech.Web.Models
{
    public class MyAccountViewModel
    {
        public string Section { get; set; } = "Orders";
        public List<Order> Orders { get; set; } = new List<Order>();
        public User? User { get; set; }
        
        // Değerlendirmelerim
        public List<ReviewViewModel> Reviews { get; set; } = new List<ReviewViewModel>();
        
        // Satıcı Mesajları
        public List<MessageViewModel> Messages { get; set; } = new List<MessageViewModel>();
        
        // Tekrar Satın Al - Geçmiş siparişlerdeki ürünler
        public List<ReorderProductViewModel> ReorderProducts { get; set; } = new List<ReorderProductViewModel>();
        
        // Önceden Gezdiklerim
        public List<BrowsingHistoryViewModel> BrowsingHistory { get; set; } = new List<BrowsingHistoryViewModel>();
        
        // Kayıtlı Kartlarım
        public List<SavedCardViewModel> SavedCards { get; set; } = new List<SavedCardViewModel>();
        
        // Duyuru Tercihlerim
        public NotificationPreferencesViewModel NotificationPreferences { get; set; } = new NotificationPreferencesViewModel();
        
        // Raporlarım
        public ReportsViewModel? ReportsData { get; set; }
    }
    
    // Değerlendirme Model
    public class ReviewViewModel
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = "";
        public string ProductImage { get; set; } = "";
        public int Rating { get; set; }
        public string Comment { get; set; } = "";
        public DateTime CreatedAt { get; set; }
    }
    
    // Satıcı Mesaj Model
    public class MessageViewModel
    {
        public int Id { get; set; }
        public string SellerName { get; set; } = "";
        public string Subject { get; set; } = "";
        public string LastMessage { get; set; } = "";
        public DateTime LastMessageDate { get; set; }
        public bool IsRead { get; set; }
    }
    
    // Tekrar Satın Al Ürün Model
    public class ReorderProductViewModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = "";
        public string ProductImage { get; set; } = "";
        public decimal Price { get; set; }
        public DateTime LastPurchaseDate { get; set; }
        public int PurchaseCount { get; set; }
    }
    
    // Tarama Geçmişi Model
    public class BrowsingHistoryViewModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = "";
        public string ProductImage { get; set; } = "";
        public decimal Price { get; set; }
        public DateTime ViewedAt { get; set; }
    }
    
    // Kayıtlı Kart Model
    public class SavedCardViewModel
    {
        public int Id { get; set; }
        public string CardNumber { get; set; } = ""; // Maskelenmiş: **** **** **** 1234
        public string CardHolderName { get; set; } = "";
        public string ExpiryDate { get; set; } = ""; // MM/YY
        public string CardType { get; set; } = ""; // Visa, MasterCard, etc.
        public bool IsDefault { get; set; }
    }
    
    // Bildirim Tercihleri Model
    public class NotificationPreferencesViewModel
    {
        public bool EmailNotifications { get; set; } = true;
        public bool SmsNotifications { get; set; } = false;
        public bool PushNotifications { get; set; } = true;
        public bool OrderUpdates { get; set; } = true;
        public bool PromotionalEmails { get; set; } = true;
        public bool PriceAlerts { get; set; } = true;
        public bool NewsletterSubscription { get; set; } = true;
    }

    // Review Update/Delete Request Models
    public class UpdateReviewRequest
    {
        [JsonPropertyName("reviewId")]
        public int ReviewId { get; set; }
        
        [JsonPropertyName("rating")]
        public int Rating { get; set; }
        
        [JsonPropertyName("comment")]
        public string Comment { get; set; } = string.Empty;
    }

    public class DeleteReviewRequest
    {
        [JsonPropertyName("reviewId")]
        public int ReviewId { get; set; }
    }

    // Card Request Models
    public class AddCardRequest
    {
        [JsonPropertyName("cardNumber")]
        public string? CardNumber { get; set; }
        
        [JsonPropertyName("cardHolderName")]
        public string? CardHolderName { get; set; }
        
        [JsonPropertyName("expiryMonth")]
        public string? ExpiryMonth { get; set; }
        
        [JsonPropertyName("expiryYear")]
        public string? ExpiryYear { get; set; }
    }

    public class DeleteCardRequest
    {
        [JsonPropertyName("cardId")]
        public int CardId { get; set; }
    }

    public class SetDefaultCardRequest
    {
        [JsonPropertyName("cardId")]
        public int CardId { get; set; }
    }
}
