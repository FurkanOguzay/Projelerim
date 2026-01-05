using Microsoft.EntityFrameworkCore;
using UpexTech.Data;
using UpexTech.Entity;

namespace UpexTech.Business.Services
{
    public interface IOrderService
    {
        Task<Order?> CreateOrderFromCartAsync(int userId, string shippingAddress, string? notes = null);
        Task<Order?> GetOrderByIdAsync(int orderId);
        Task<IEnumerable<Order>> GetOrdersByUserIdAsync(int userId);
        Task<bool> UpdateOrderStatusAsync(int orderId, OrderStatus status);
        
        // Admin Panel Methods
        Task<IEnumerable<Order>> GetAllOrdersAsync();
        Task<(IEnumerable<Order> Orders, int TotalCount)> GetOrdersPagedAsync(
            int page, int pageSize, OrderStatus? status = null, 
            DateTime? fromDate = null, DateTime? toDate = null, string? search = null);
        Task<Dictionary<OrderStatus, int>> GetOrderCountByStatusAsync();
        
        // Dashboard Statistics Methods
        Task<decimal> GetDailyRevenueAsync();
        Task<decimal> GetB2BRevenueAsync();
        Task<decimal> GetB2CRevenueAsync();
        Task<int> GetPendingOrdersCountAsync();
        Task<int> GetReturnRequestsCountAsync();
        Task<Dictionary<string, object>> GetWeeklySalesDataAsync();
        Task<Dictionary<string, int>> GetTopSellingCategoriesAsync(int count = 5);
        Task<IEnumerable<Order>> GetRecentOrdersAsync(int count = 5);
    }

    public class OrderService : IOrderService
    {
        private readonly UpexTechDbContext _context;
        private readonly ICartService _cartService;

        public OrderService(UpexTechDbContext context, ICartService cartService)
        {
            _context = context;
            _cartService = cartService;
        }

        public async Task<Order?> CreateOrderFromCartAsync(int userId, string shippingAddress, string? notes = null)
        {
            // Sepeti getir
            var cart = await _cartService.GetCartByUserIdAsync(userId);
            if (cart == null || !cart.Items.Any())
            {
                return null;
            }

            // Sipariş numarası oluştur
            var orderNumber = GenerateOrderNumber();

            // Toplam tutarı hesapla
            var totalAmount = cart.Items.Sum(i => i.TotalPrice);

            // Sipariş oluştur
            var order = new Order
            {
                OrderNumber = orderNumber,
                UserId = userId,
                TotalAmount = totalAmount,
                Status = OrderStatus.Pending,
                ShippingAddress = shippingAddress,
                Notes = notes,
                CreatedAt = DateTime.Now,
                IsActive = true
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // Sipariş ürünlerini ekle
            foreach (var cartItem in cart.Items)
            {
                var orderItem = new OrderItem
                {
                    OrderId = order.Id,
                    ProductId = cartItem.ProductId,
                    Quantity = cartItem.Quantity,
                    UnitPrice = cartItem.UnitPrice,
                    CreatedAt = DateTime.Now,
                    IsActive = true
                };

                _context.OrderItems.Add(orderItem);

                // Stoktan düş
                var product = await _context.Products.FindAsync(cartItem.ProductId);
                if (product != null)
                {
                    product.Stock -= cartItem.Quantity;
                    product.UpdatedAt = DateTime.Now;
                }
            }

            await _context.SaveChangesAsync();

            // Sepeti temizle
            await _cartService.ClearCartAsync(userId);

            // Siparişi navigation properties ile birlikte döndür
            return await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.Id == order.Id);
        }

        public async Task<Order?> GetOrderByIdAsync(int orderId)
        {
            return await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.Id == orderId && o.IsActive);
        }

        public async Task<IEnumerable<Order>> GetOrdersByUserIdAsync(int userId)
        {
            return await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Where(o => o.UserId == userId && o.IsActive)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
        }

        public async Task<bool> UpdateOrderStatusAsync(int orderId, OrderStatus status)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null) return false;

            order.Status = status;
            order.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();

            return true;
        }

        private string GenerateOrderNumber()
        {
            // Format: ORD-YYYYMMDD-XXXXX (örnek: ORD-20251216-00001)
            var date = DateTime.Now.ToString("yyyyMMdd");
            var random = new Random().Next(10000, 99999);
            return $"ORD-{date}-{random}";
        }

        // Admin Panel Method Implementations
        public async Task<IEnumerable<Order>> GetAllOrdersAsync()
        {
            return await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Include(o => o.User)
                .Where(o => o.IsActive)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
        }

        public async Task<(IEnumerable<Order> Orders, int TotalCount)> GetOrdersPagedAsync(
            int page, int pageSize, OrderStatus? status = null,
            DateTime? fromDate = null, DateTime? toDate = null, string? search = null)
        {
            var query = _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Include(o => o.User)
                .Where(o => o.IsActive)
                .AsQueryable();

            // Status filter
            if (status.HasValue)
            {
                query = query.Where(o => o.Status == status.Value);
            }

            // Date filters
            if (fromDate.HasValue)
            {
                query = query.Where(o => o.CreatedAt >= fromDate.Value);
            }
            if (toDate.HasValue)
            {
                query = query.Where(o => o.CreatedAt <= toDate.Value.AddDays(1));
            }

            // Search filter (order number, customer name, phone)
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.ToLower();
                query = query.Where(o => 
                    o.OrderNumber.ToLower().Contains(search) ||
                    (o.User.FullName != null && o.User.FullName.ToLower().Contains(search)) ||
                    (o.User.Phone != null && o.User.Phone.Contains(search)));
            }

            var totalCount = await query.CountAsync();
            
            var orders = await query
                .OrderByDescending(o => o.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (orders, totalCount);
        }

        public async Task<Dictionary<OrderStatus, int>> GetOrderCountByStatusAsync()
        {
            var counts = await _context.Orders
                .Where(o => o.IsActive)
                .GroupBy(o => o.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();

            return counts.ToDictionary(x => x.Status, x => x.Count);
        }

        // Dashboard Statistics Method Implementations
        public async Task<decimal> GetDailyRevenueAsync()
        {
            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);

            var revenue = await _context.Orders
                .Where(o => o.IsActive && 
                            o.CreatedAt >= today && 
                            o.CreatedAt < tomorrow &&
                            o.Status != OrderStatus.Cancelled)
                .SumAsync(o => o.TotalAmount);

            return revenue;
        }

        public async Task<decimal> GetB2BRevenueAsync()
        {
            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);

            var revenue = await _context.Orders
                .Include(o => o.User)
                .Where(o => o.IsActive && 
                            o.CreatedAt >= today && 
                            o.CreatedAt < tomorrow &&
                            o.Status != OrderStatus.Cancelled &&
                            o.User.Role == UserRole.B2B)
                .SumAsync(o => o.TotalAmount);

            return revenue;
        }

        public async Task<decimal> GetB2CRevenueAsync()
        {
            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);

            var revenue = await _context.Orders
                .Include(o => o.User)
                .Where(o => o.IsActive && 
                            o.CreatedAt >= today && 
                            o.CreatedAt < tomorrow &&
                            o.Status != OrderStatus.Cancelled &&
                            o.User.Role == UserRole.B2C)
                .SumAsync(o => o.TotalAmount);

            return revenue;
        }

        public async Task<int> GetPendingOrdersCountAsync()
        {
            return await _context.Orders
                .Where(o => o.IsActive && o.Status == OrderStatus.Pending)
                .CountAsync();
        }

        public async Task<int> GetReturnRequestsCountAsync()
        {
            return await _context.Orders
                .Where(o => o.IsActive && o.Status == OrderStatus.Returned)
                .CountAsync();
        }

        public async Task<Dictionary<string, object>> GetWeeklySalesDataAsync()
        {
            var today = DateTime.Today;
            var b2bSales = new decimal[7];
            var b2cSales = new decimal[7];
            var labels = new string[7];

            // Türkçe gün isimleri
            var turkishDayNames = new[] { "Paz", "Pzt", "Sal", "Çar", "Per", "Cum", "Cmt" };

            // Son 7 günü geriye doğru hesapla
            for (int i = 0; i < 7; i++)
            {
                var date = today.AddDays(-6 + i); // 6 gün önce ile başla, bugüne kadar
                var nextDay = date.AddDays(1);

                // Gün etiketini al
                labels[i] = turkishDayNames[(int)date.DayOfWeek];

                // B2B satışları
                b2bSales[i] = await _context.Orders
                    .Include(o => o.User)
                    .Where(o => o.IsActive &&
                                o.CreatedAt >= date &&
                                o.CreatedAt < nextDay &&
                                o.Status != OrderStatus.Cancelled &&
                                o.User.Role == UserRole.B2B)
                    .SumAsync(o => (decimal?)o.TotalAmount) ?? 0;

                // B2C satışları
                b2cSales[i] = await _context.Orders
                    .Include(o => o.User)
                    .Where(o => o.IsActive &&
                                o.CreatedAt >= date &&
                                o.CreatedAt < nextDay &&
                                o.Status != OrderStatus.Cancelled &&
                                o.User.Role == UserRole.B2C)
                    .SumAsync(o => (decimal?)o.TotalAmount) ?? 0;
            }

            return new Dictionary<string, object>
            {
                { "b2b", b2bSales },
                { "b2c", b2cSales },
                { "labels", labels }
            };
        }

        public async Task<Dictionary<string, int>> GetTopSellingCategoriesAsync(int count = 5)
        {
            // Tüm sipariş ürünlerinden kategorilere göre satış miktarlarını hesapla
            var categorySales = await _context.OrderItems
                .Include(oi => oi.Product)
                    .ThenInclude(p => p.Category)
                .Where(oi => oi.IsActive && 
                             oi.Order.IsActive && 
                             oi.Order.Status != OrderStatus.Cancelled &&
                             oi.Product.Category != null)
                .GroupBy(oi => oi.Product.Category!.Name)
                .Select(g => new
                {
                    CategoryName = g.Key,
                    TotalQuantity = g.Sum(oi => oi.Quantity)
                })
                .OrderByDescending(x => x.TotalQuantity)
                .Take(count)
                .ToListAsync();

            // Toplam satış miktarını hesapla
            var totalSales = categorySales.Sum(x => x.TotalQuantity);

            // Yüzdeleri hesapla ve dictionary'e dönüştür
            var result = new Dictionary<string, int>();
            foreach (var item in categorySales)
            {
                var percentage = totalSales > 0 ? (int)Math.Round((double)item.TotalQuantity / totalSales * 100) : 0;
                result[item.CategoryName] = percentage;
            }

            return result;
        }

        public async Task<IEnumerable<Order>> GetRecentOrdersAsync(int count = 5)
        {
            return await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Where(o => o.IsActive)
                .OrderByDescending(o => o.CreatedAt)
                .Take(count)
                .ToListAsync();
        }
    }
}
