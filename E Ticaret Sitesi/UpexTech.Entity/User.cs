namespace UpexTech.Entity
{
    public class User : BaseEntity
    {
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public UserRole Role { get; set; } = UserRole.B2C;
        public UserStatus Status { get; set; } = UserStatus.Active;

        // Bayi için ek alanlar
        public string? CompanyName { get; set; }
        public string? TaxNumber { get; set; }
        public string? Address { get; set; }

        // B2B Tier ve Limit
        public CustomerTier Tier { get; set; } = CustomerTier.Standard;
        public decimal? CreditLimit { get; set; }
        public string? TaxDocument { get; set; }  // Vergi levhası dosya adı
        
        // Fiyat Listesi
        public int? PriceListId { get; set; }
        public string? PriceListName { get; set; }

        // Navigation Properties
        public virtual ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
        public virtual ICollection<FavoriteCollection> FavoriteCollections { get; set; } = new List<FavoriteCollection>();
        public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();

        public string FullName => $"{FirstName} {LastName}";
    }
}

