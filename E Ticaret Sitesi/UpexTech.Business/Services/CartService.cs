using Microsoft.EntityFrameworkCore;
using UpexTech.Data;
using UpexTech.Entity;

namespace UpexTech.Business.Services
{
    public interface ICartService
    {
        Task<Cart?> GetCartByUserIdAsync(int userId);
        Task<Cart> GetOrCreateCartAsync(int userId);
        Task<CartItem?> AddToCartAsync(int userId, int productId, int quantity, bool isB2B, PriceList? priceList = null);
        Task<CartItem?> UpdateQuantityAsync(int userId, int productId, int quantity);
        Task<bool> RemoveFromCartAsync(int userId, int productId);
        Task<bool> ClearCartAsync(int userId);
        Task<int> GetCartItemCountAsync(int userId);
        Task<decimal> GetCartTotalAsync(int userId, bool isB2B, PriceList? priceList = null);
    }

    public class CartService : ICartService
    {
        private readonly UpexTechDbContext _context;

        public CartService(UpexTechDbContext context)
        {
            _context = context;
        }

        public async Task<Cart?> GetCartByUserIdAsync(int userId)
        {
            var cart = await _context.Carts
                .FirstOrDefaultAsync(c => c.UserId == userId && c.IsActive);

            if (cart == null) return null;

            // Items'ları ayrı sorgu ile al
            var items = await _context.CartItems
                .Include(i => i.Product)
                    .ThenInclude(p => p.Brand)
                .Include(i => i.Product)
                    .ThenInclude(p => p.Category)
                .Where(i => i.CartId == cart.Id && i.IsActive)
                .ToListAsync();

            cart.Items = items;

            return cart;
        }

        public async Task<Cart> GetOrCreateCartAsync(int userId)
        {
            var cart = await _context.Carts
                .FirstOrDefaultAsync(c => c.UserId == userId && c.IsActive);
            
            if (cart == null)
            {
                cart = new Cart
                {
                    UserId = userId,
                    CreatedAt = DateTime.Now,
                    IsActive = true
                };
                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
            }

            return cart;
        }

        public async Task<CartItem?> AddToCartAsync(int userId, int productId, int quantity, bool isB2B, PriceList? priceList = null)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null || !product.IsActive) return null;

            // Stok kontrolü
            if (quantity > product.Stock)
                quantity = product.Stock;

            if (quantity <= 0) return null;

            // Fiyat hesapla
            decimal unitPrice = CalculatePrice(product, isB2B, priceList);

            var cart = await GetOrCreateCartAsync(userId);
            
            // Mevcut item var mı kontrol et (pasif dahil)
            var existingItem = await _context.CartItems
                .FirstOrDefaultAsync(ci => ci.CartId == cart.Id && ci.ProductId == productId);

            if (existingItem != null)
            {
                // Mevcut item'ı güncelle
                var newQuantity = existingItem.IsActive ? existingItem.Quantity + quantity : quantity;
                
                // Stok kontrolü
                if (newQuantity > product.Stock)
                    newQuantity = product.Stock;

                existingItem.Quantity = newQuantity;
                existingItem.UnitPrice = unitPrice;
                existingItem.IsActive = true;
                existingItem.UpdatedAt = DateTime.Now;
            }
            else
            {
                // Yeni item ekle
                existingItem = new CartItem
                {
                    CartId = cart.Id,
                    ProductId = productId,
                    Quantity = quantity,
                    UnitPrice = unitPrice,
                    CreatedAt = DateTime.Now,
                    IsActive = true
                };
                _context.CartItems.Add(existingItem);
            }

            await _context.SaveChangesAsync();
            
            // Product bilgisini yükle
            await _context.Entry(existingItem).Reference(ci => ci.Product).LoadAsync();
            
            return existingItem;
        }

        public async Task<CartItem?> UpdateQuantityAsync(int userId, int productId, int quantity)
        {
            var cart = await GetCartByUserIdAsync(userId);
            if (cart == null) return null;

            var cartItem = cart.Items.FirstOrDefault(i => i.ProductId == productId);
            if (cartItem == null) return null;

            var product = await _context.Products.FindAsync(productId);
            if (product == null) return null;

            // Stok kontrolü
            if (quantity > product.Stock)
                quantity = product.Stock;

            if (quantity <= 0)
            {
                // Miktarı 0 veya altına düşürürse kaldır
                cartItem.IsActive = false;
                cartItem.UpdatedAt = DateTime.Now;
            }
            else
            {
                cartItem.Quantity = quantity;
                cartItem.UpdatedAt = DateTime.Now;
            }

            await _context.SaveChangesAsync();
            return cartItem;
        }

        public async Task<bool> RemoveFromCartAsync(int userId, int productId)
        {
            var cart = await GetCartByUserIdAsync(userId);
            if (cart == null) return false;

            var cartItem = cart.Items.FirstOrDefault(i => i.ProductId == productId);
            if (cartItem == null) return false;

            cartItem.IsActive = false;
            cartItem.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();
            
            return true;
        }

        public async Task<bool> ClearCartAsync(int userId)
        {
            var cart = await GetCartByUserIdAsync(userId);
            if (cart == null) return false;

            foreach (var item in cart.Items)
            {
                item.IsActive = false;
                item.UpdatedAt = DateTime.Now;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> GetCartItemCountAsync(int userId)
        {
            try
            {
                // Önce cart var mı kontrol et
                var cart = await _context.Carts
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.UserId == userId && c.IsActive);

                if (cart == null) return 0;

                // Sonra items'ları say
                return await _context.CartItems
                    .AsNoTracking()
                    .Where(ci => ci.CartId == cart.Id && ci.IsActive)
                    .SumAsync(ci => ci.Quantity);
            }
            catch
            {
                return 0;
            }
        }

        public async Task<decimal> GetCartTotalAsync(int userId, bool isB2B, PriceList? priceList = null)
        {
            var cart = await GetCartByUserIdAsync(userId);
            if (cart == null) return 0;

            decimal total = 0;
            foreach (var item in cart.Items)
            {
                if (item.Product != null)
                {
                    decimal price = CalculatePrice(item.Product, isB2B, priceList);
                    total += item.Quantity * price;
                }
            }
            return total;
        }

        /// <summary>
        /// Ürün fiyatını PriceList'e göre hesaplar
        /// </summary>
        private decimal CalculatePrice(Product product, bool isB2B, PriceList? priceList)
        {
            // Varsayılan fiyat (fallback)
            decimal defaultPrice = isB2B ? product.PriceB2B : product.PriceB2C;
            
            if (priceList == null)
            {
                return defaultPrice;
            }

            // PurchasePrice 0 ise fallback kullan
            decimal basePrice = product.PurchasePrice > 0 ? product.PurchasePrice : defaultPrice;
            
            // Factor uygula
            decimal calculatedPrice = basePrice * priceList.Factor;
            
            // Yuvarlama uygula
            calculatedPrice = priceList.Rounding switch
            {
                RoundingMethod.Ending90 => Math.Floor(calculatedPrice) + 0.90m,
                RoundingMethod.Ending99 => Math.Floor(calculatedPrice) + 0.99m,
                RoundingMethod.NearestFive => Math.Round(calculatedPrice / 5) * 5,
                _ => Math.Round(calculatedPrice, 2)
            };
            
            // Hesaplanan fiyat 0 ise fallback kullan
            return calculatedPrice > 0 ? calculatedPrice : defaultPrice;
        }
    }
}
