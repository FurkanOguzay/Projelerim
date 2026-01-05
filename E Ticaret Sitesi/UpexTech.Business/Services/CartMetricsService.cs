using Microsoft.EntityFrameworkCore;
using UpexTech.Business.DTOs;
using UpexTech.Data;

namespace UpexTech.Business.Services
{
    public class CartMetricsService : ICartMetricsService
    {
        private readonly UpexTechDbContext _context;

        public CartMetricsService(UpexTechDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<CartBehaviorReportDto>> GetCartBehaviorReportAsync()
        {
            // Tüm ürünleri metrikleriyle birlikte getir
            var products = await _context.Products
                .Include(p => p.Category)
                .Where(p => p.IsActive)
                .ToListAsync();

            var reportData = new List<CartBehaviorReportDto>();

            foreach (var product in products)
            {
                // 1. Brüt Sepete Eklenme Sayısı (aktif sepetteki toplam adet)
                var grossCartAddCount = await _context.CartItems
                    .Where(ci => ci.ProductId == product.Id && ci.IsActive)
                    .SumAsync(ci => ci.Quantity);

                // 2. Şu an kaç kullanıcının sepetinde (distinct user count)
                var currentCartUserCount = await _context.CartItems
                    .Include(ci => ci.Cart)
                    .Where(ci => ci.ProductId == product.Id && ci.IsActive && ci.Cart.IsActive)
                    .Select(ci => ci.Cart.UserId)
                    .Distinct()
                    .CountAsync();

                // 3. Aktif Favori Sayısı
                var currentFavoriteCount = await _context.Favorites
                    .Where(f => f.ProductId == product.Id && f.IsActive)
                    .CountAsync();

                // 4. Net Satış Adedi (tamamlanmış siparişlerden)
                var netSalesCount = await _context.OrderItems
                    .Include(oi => oi.Order)
                    .Where(oi => oi.ProductId == product.Id && 
                                oi.Order.IsActive &&
                                (oi.Order.Status == Entity.OrderStatus.Confirmed ||
                                 oi.Order.Status == Entity.OrderStatus.Shipped ||
                                 oi.Order.Status == Entity.OrderStatus.Delivered))
                    .SumAsync(oi => (int?)oi.Quantity) ?? 0;

                // 5. Net Ciro (tamamlanmış siparişlerden)
                var netRevenue = await _context.OrderItems
                    .Include(oi => oi.Order)
                    .Where(oi => oi.ProductId == product.Id && 
                                oi.Order.IsActive &&
                                (oi.Order.Status == Entity.OrderStatus.Confirmed ||
                                 oi.Order.Status == Entity.OrderStatus.Shipped ||
                                 oi.Order.Status == Entity.OrderStatus.Delivered))
                    .SumAsync(oi => (decimal?)(oi.Quantity * oi.UnitPrice)) ?? 0;

                reportData.Add(new CartBehaviorReportDto
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    SKU = $"SKU-{product.Id:D6}", // Product ID'den SKU oluştur
                    CategoryName = product.Category?.Name ?? "Kategori Yok",
                    GrossCartAddCount = grossCartAddCount,
                    CurrentCartUserCount = currentCartUserCount,
                    CurrentFavoriteCount = currentFavoriteCount,
                    NetSalesCount = netSalesCount,
                    NetRevenue = netRevenue,
                    CurrentStock = product.Stock
                });
            }

            // Net Satış Adedi'ne göre azalan sırada döndür (en çok satandan başla)
            return reportData.OrderByDescending(r => r.NetSalesCount).ToList();
        }
    }
}
