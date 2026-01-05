using UpexTech.Entity;

namespace UpexTech.Web.Models
{
    public class HomeViewModel
    {
        public List<Product> PopularProducts { get; set; } = new();
        public List<Product> ImmediateDeliveryProducts { get; set; } = new();
        public List<Category> Categories { get; set; } = new();
        public bool IsLoggedIn { get; set; }
        public UserRole? UserRole { get; set; }
        
        /// <summary>
        /// Giriş yapan kullanıcının atanmış fiyat listesi
        /// </summary>
        public PriceList? UserPriceList { get; set; }
    }

    public class ProductCardViewModel
    {
        public Product Product { get; set; } = null!;
        public bool ShowPrice { get; set; }
        public bool IsB2B { get; set; }
        public bool IsFavorite { get; set; }
        public bool IsLoggedIn { get; set; }
        
        /// <summary>
        /// PriceList'e göre hesaplanmış fiyat. PriceList yoksa varsayılan fiyat.
        /// </summary>
        public decimal CalculatedPrice { get; set; }
    }
}
