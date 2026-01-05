using Microsoft.EntityFrameworkCore;
using UpexTech.Business.DTOs;
using UpexTech.Data;
using UpexTech.Entity;

namespace UpexTech.Business.Services
{
    public interface ISalesReportService
    {
        Task<SalesReportSummaryDto> GetSalesSummaryAsync(DateTime startDate, DateTime endDate);
        Task<List<SalesTrendDto>> GetSalesTrendAsync(DateTime startDate, DateTime endDate);
        Task<List<TopProductDto>> GetTopProductsAsync(DateTime startDate, DateTime endDate, int count = 10);
        Task<List<CategoryDistributionDto>> GetCategoryDistributionAsync(DateTime startDate, DateTime endDate);
        Task<List<StockTurnoverDto>> GetStockTurnoverAsync(int days = 30);
    }

    public class SalesReportService : ISalesReportService
    {
        private readonly UpexTechDbContext _context;

        public SalesReportService(UpexTechDbContext context)
        {
            _context = context;
        }

        public async Task<SalesReportSummaryDto> GetSalesSummaryAsync(DateTime startDate, DateTime endDate)
        {
            var orders = await _context.Orders
                .Where(o => o.IsActive && o.CreatedAt >= startDate && o.CreatedAt <= endDate)
                .Include(o => o.OrderItems)
                .ToListAsync();

            var totalRevenue = orders.Sum(o => o.TotalAmount);
            var totalOrders = orders.Count;
            var totalProductsSold = orders.SelectMany(o => o.OrderItems).Sum(oi => oi.Quantity);
            var averageOrderValue = totalOrders > 0 ? totalRevenue / totalOrders : 0;

            return new SalesReportSummaryDto
            {
                TotalRevenue = totalRevenue,
                TotalOrders = totalOrders,
                TotalProductsSold = totalProductsSold,
                AverageOrderValue = averageOrderValue
            };
        }

        public async Task<List<SalesTrendDto>> GetSalesTrendAsync(DateTime startDate, DateTime endDate)
        {
            var orders = await _context.Orders
                .Where(o => o.IsActive && o.CreatedAt >= startDate && o.CreatedAt <= endDate)
                .ToListAsync();

            // Tarih aralığına göre gruplama mantığını belirle
            var daysDiff = (endDate - startDate).Days;
            
            List<SalesTrendDto> trend;
            
            if (daysDiff <= 31)
            {
                // Günlük gruplama
                trend = orders
                    .GroupBy(o => o.CreatedAt.Date)
                    .Select(g => new SalesTrendDto
                    {
                        Date = g.Key.ToString("dd MMM"),
                        Revenue = g.Sum(o => o.TotalAmount),
                        OrderCount = g.Count()
                    })
                    .OrderBy(t => t.Date)
                    .ToList();
                    
                // Eksik günleri 0 ile doldur
                var allDates = Enumerable.Range(0, daysDiff + 1)
                    .Select(d => startDate.AddDays(d).Date)
                    .ToList();
                    
                foreach (var date in allDates)
                {
                    var dateStr = date.ToString("dd MMM");
                    if (!trend.Any(t => t.Date == dateStr))
                    {
                        trend.Add(new SalesTrendDto
                        {
                            Date = dateStr,
                            Revenue = 0,
                            OrderCount = 0
                        });
                    }
                }
                
                trend = trend.OrderBy(t => DateTime.ParseExact(t.Date, "dd MMM", System.Globalization.CultureInfo.GetCultureInfo("tr-TR"))).ToList();
            }
            else
            {
                // Aylık gruplama
                trend = orders
                    .GroupBy(o => new { o.CreatedAt.Year, o.CreatedAt.Month })
                    .Select(g => new SalesTrendDto
                    {
                        Date = $"{g.Key.Month:00}/{g.Key.Year}",
                        Revenue = g.Sum(o => o.TotalAmount),
                        OrderCount = g.Count()
                    })
                    .OrderBy(t => t.Date)
                    .ToList();
            }

            return trend;
        }

        public async Task<List<TopProductDto>> GetTopProductsAsync(DateTime startDate, DateTime endDate, int count = 10)
        {
            var topProducts = await _context.OrderItems
                .Include(oi => oi.Order)
                .Include(oi => oi.Product)
                    .ThenInclude(p => p.Category)
                .Where(oi => oi.Order.IsActive && oi.Order.CreatedAt >= startDate && oi.Order.CreatedAt <= endDate)
                .GroupBy(oi => new { oi.ProductId, oi.Product.Name, CategoryName = oi.Product.Category.Name })
                .Select(g => new TopProductDto
                {
                    ProductId = g.Key.ProductId,
                    ProductName = g.Key.Name,
                    CategoryName = g.Key.CategoryName,
                    SalesCount = g.Sum(oi => oi.Quantity),
                    Revenue = g.Sum(oi => oi.Quantity * oi.UnitPrice)
                })
                .OrderByDescending(p => p.Revenue)
                .Take(count)
                .ToListAsync();

            return topProducts;
        }

        public async Task<List<CategoryDistributionDto>> GetCategoryDistributionAsync(DateTime startDate, DateTime endDate)
        {
            var categoryData = await _context.OrderItems
                .Include(oi => oi.Order)
                .Include(oi => oi.Product)
                    .ThenInclude(p => p.Category)
                .Where(oi => oi.Order.IsActive && oi.Order.CreatedAt >= startDate && oi.Order.CreatedAt <= endDate)
                .GroupBy(oi => oi.Product.Category.Name)
                .Select(g => new CategoryDistributionDto
                {
                    CategoryName = g.Key,
                    Revenue = g.Sum(oi => oi.Quantity * oi.UnitPrice),
                    Percentage = 0 // Will be calculated below
                })
                .ToListAsync();

            var totalRevenue = categoryData.Sum(c => c.Revenue);
            
            foreach (var category in categoryData)
            {
                category.Percentage = totalRevenue > 0 
                    ? Math.Round((category.Revenue / totalRevenue) * 100, 1) 
                    : 0;
            }

            return categoryData.OrderByDescending(c => c.Revenue).ToList();
        }

        public async Task<List<StockTurnoverDto>> GetStockTurnoverAsync(int days = 30)
        {
            var startDate = DateTime.Now.AddDays(-days);
            var endDate = DateTime.Now;

            var products = await _context.Products
                .Include(p => p.Category)
                .Where(p => p.IsActive)
                .ToListAsync();

            var salesData = await _context.OrderItems
                .Include(oi => oi.Order)
                .Where(oi => oi.Order.IsActive && oi.Order.CreatedAt >= startDate && oi.Order.CreatedAt <= endDate)
                .GroupBy(oi => oi.ProductId)
                .Select(g => new { ProductId = g.Key, TotalSold = g.Sum(oi => oi.Quantity) })
                .ToListAsync();

            var stockTurnover = products.Select(p =>
            {
                var sold = salesData.FirstOrDefault(s => s.ProductId == p.Id)?.TotalSold ?? 0;
                var averageStock = p.Stock + (sold / 2.0);
                var turnoverRate = averageStock > 0 ? (double)sold / averageStock : 0;

                string status;
                if (turnoverRate >= 0.5) status = "Hızlı";
                else if (turnoverRate >= 0.2) status = "Normal";
                else status = "Yavaş";

                return new StockTurnoverDto
                {
                    ProductId = p.Id,
                    ProductName = p.Name,
                    CategoryName = p.Category?.Name ?? "Kategorisiz",
                    CurrentStock = p.Stock,
                    TotalSold = sold,
                    TurnoverRate = (decimal)Math.Round(turnoverRate, 2),
                    TurnoverStatus = status
                };
            })
            .OrderByDescending(s => s.TurnoverRate)
            .ToList();

            return stockTurnover;
        }
    }
}
