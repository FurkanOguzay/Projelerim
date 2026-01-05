using System;

namespace UpexTech.Entity
{
    public class SavedCard : BaseEntity
    {
        public int UserId { get; set; }
        public string CardNumber { get; set; } = string.Empty; // Sadece son 4 hanesi saklanacak: **** **** **** 1234
        public string CardHolderName { get; set; } = string.Empty;
        public string ExpiryMonth { get; set; } = string.Empty; // 01-12
        public string ExpiryYear { get; set; } = string.Empty;  // 24, 25, 26...
        public string CardType { get; set; } = "Visa"; // Visa, MasterCard, Troy
        public bool IsDefault { get; set; }
        
        // Navigation Property
        public virtual User User { get; set; } = null!;
        
        // Computed property for display
        public string ExpiryDate => $"{ExpiryMonth}/{ExpiryYear}";
    }
}
