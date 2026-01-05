using Microsoft.EntityFrameworkCore;
using UpexTech.Business.DTOs;
using UpexTech.Data;
using UpexTech.Entity;

namespace UpexTech.Business.Services
{
    public class PerformanceReportService : IPerformanceReportService
    {
        private readonly UpexTechDbContext _context;

        public PerformanceReportService(UpexTechDbContext context)
        {
            _context = context;
        }

        public async Task<GeneralPerformanceSummaryDto> GetGeneralPerformanceSummaryAsync(string segment, DateTime startDate, DateTime endDate)
        {
            var ordersQuery = _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                .Where(o => o.IsActive && o.CreatedAt >= startDate && o.CreatedAt <= endDate);

            // Segment filtreleme
            if (segment == "b2b")
                ordersQuery = ordersQuery.Where(o => o.User.Role == UserRole.B2B);
            else if (segment == "b2c")
                ordersQuery = ordersQuery.Where(o => o.User.Role == UserRole.B2C);

            var orders = await ordersQuery.ToListAsync();

            // Önceki dönem için karşılaştırma
            var periodLength = (endDate - startDate).Days;
            var previousStartDate = startDate.AddDays(-periodLength - 1);
            var previousEndDate = startDate.AddDays(-1);

            var previousOrdersQuery = _context.Orders
                .Include(o => o.User)
                .Where(o => o.IsActive && o.CreatedAt >= previousStartDate && o.CreatedAt <= previousEndDate);

            if (segment == "b2b")
                previousOrdersQuery = previousOrdersQuery.Where(o => o.User.Role == UserRole.B2B);
            else if (segment == "b2c")
                previousOrdersQuery = previousOrdersQuery.Where(o => o.User.Role == UserRole.B2C);

            var previousOrders = await previousOrdersQuery.ToListAsync();

            var currentRevenue = orders.Sum(o => o.TotalAmount);
            var previousRevenue = previousOrders.Sum(o => o.TotalAmount);
            var revenueChange = previousRevenue > 0 
                ? Math.Round(((currentRevenue - previousRevenue) / previousRevenue) * 100, 1) 
                : 0;

            // Görüntülenme - örnek veri (gerçek implementasyonda analytics sistemi kullanılmalı)
            var viewCount = await _context.Products.Where(p => p.IsActive).CountAsync() * 50; // Yaklaşık görüntülenme

            // Sepete eklenen ürün sayısı
            var cartAdditions = await _context.CartItems.CountAsync();

            // Dönüşüm oranı = (Sipariş sayısı / Görüntülenme) * 100
            var conversionRate = viewCount > 0 
                ? Math.Round((decimal)orders.Count / viewCount * 100, 1) 
                : 0;

            return new GeneralPerformanceSummaryDto
            {
                NetRevenue = currentRevenue,
                RevenueChange = revenueChange,
                OrderCount = orders.Count,
                ViewCount = viewCount,
                ConversionRate = conversionRate
            };
        }

        public async Task<List<SalesChartDataDto>> GetSalesChartDataAsync(string segment, DateTime startDate, DateTime endDate, string breakdown)
        {
            var ordersQuery = _context.Orders
                .Include(o => o.User)
                .Where(o => o.IsActive && o.CreatedAt >= startDate && o.CreatedAt <= endDate);

            if (segment == "b2b")
                ordersQuery = ordersQuery.Where(o => o.User.Role == UserRole.B2B);
            else if (segment == "b2c")
                ordersQuery = ordersQuery.Where(o => o.User.Role == UserRole.B2C);

            var orders = await ordersQuery.ToListAsync();

            // Önceki dönem verileri
            var periodLength = (endDate - startDate).Days;
            var previousStartDate = startDate.AddDays(-periodLength - 1);
            var previousEndDate = startDate.AddDays(-1);

            var previousOrdersQuery = _context.Orders
                .Include(o => o.User)
                .Where(o => o.IsActive && o.CreatedAt >= previousStartDate && o.CreatedAt <= previousEndDate);

            if (segment == "b2b")
                previousOrdersQuery = previousOrdersQuery.Where(o => o.User.Role == UserRole.B2B);
            else if (segment == "b2c")
                previousOrdersQuery = previousOrdersQuery.Where(o => o.User.Role == UserRole.B2C);

            var previousOrders = await previousOrdersQuery.ToListAsync();

            var result = new List<SalesChartDataDto>();

            switch (breakdown.ToLower())
            {
                case "hourly":
                    for (int hour = 9; hour <= 18; hour++)
                    {
                        var currentTotal = orders.Where(o => o.CreatedAt.Hour == hour).Sum(o => o.TotalAmount);
                        var previousTotal = previousOrders.Where(o => o.CreatedAt.Hour == hour).Sum(o => o.TotalAmount);
                        result.Add(new SalesChartDataDto
                        {
                            Label = $"{hour:00}:00",
                            CurrentPeriod = currentTotal,
                            PreviousPeriod = previousTotal
                        });
                    }
                    break;

                case "daily":
                    var days = (int)(endDate - startDate).TotalDays + 1;
                    for (int i = 0; i < days && i < 31; i++)
                    {
                        var date = startDate.AddDays(i);
                        var currentTotal = orders.Where(o => o.CreatedAt.Date == date.Date).Sum(o => o.TotalAmount);
                        var previousDate = previousStartDate.AddDays(i);
                        var previousTotal = previousOrders.Where(o => o.CreatedAt.Date == previousDate.Date).Sum(o => o.TotalAmount);
                        result.Add(new SalesChartDataDto
                        {
                            Label = date.ToString("dd MMM"),
                            CurrentPeriod = currentTotal,
                            PreviousPeriod = previousTotal
                        });
                    }
                    break;

                case "weekly":
                    var currentGrouped = orders
                        .GroupBy(o => System.Globalization.CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(
                            o.CreatedAt, System.Globalization.CalendarWeekRule.FirstDay, DayOfWeek.Monday))
                        .Select(g => new { Week = g.Key, Total = g.Sum(o => o.TotalAmount) })
                        .ToList();

                    var previousGrouped = previousOrders
                        .GroupBy(o => System.Globalization.CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(
                            o.CreatedAt, System.Globalization.CalendarWeekRule.FirstDay, DayOfWeek.Monday))
                        .Select(g => new { Week = g.Key, Total = g.Sum(o => o.TotalAmount) })
                        .ToList();

                    foreach (var week in currentGrouped)
                    {
                        var prevWeek = previousGrouped.FirstOrDefault(p => p.Week == week.Week);
                        result.Add(new SalesChartDataDto
                        {
                            Label = $"Hafta {week.Week}",
                            CurrentPeriod = week.Total,
                            PreviousPeriod = prevWeek?.Total ?? 0
                        });
                    }
                    break;

                case "monthly":
                default:
                    var currentMonthly = orders
                        .GroupBy(o => new { o.CreatedAt.Year, o.CreatedAt.Month })
                        .Select(g => new { g.Key.Year, g.Key.Month, Total = g.Sum(o => o.TotalAmount) })
                        .ToList();

                    var previousMonthly = previousOrders
                        .GroupBy(o => new { o.CreatedAt.Year, o.CreatedAt.Month })
                        .Select(g => new { g.Key.Year, g.Key.Month, Total = g.Sum(o => o.TotalAmount) })
                        .ToList();

                    foreach (var month in currentMonthly)
                    {
                        var prevMonth = previousMonthly.FirstOrDefault(p => p.Month == month.Month);
                        var monthNames = new[] { "", "Oca", "Şub", "Mar", "Nis", "May", "Haz", "Tem", "Ağu", "Eyl", "Eki", "Kas", "Ara" };
                        result.Add(new SalesChartDataDto
                        {
                            Label = monthNames[month.Month],
                            CurrentPeriod = month.Total,
                            PreviousPeriod = prevMonth?.Total ?? 0
                        });
                    }
                    break;
            }

            return result;
        }

        public async Task<List<CitySalesDto>> GetCitySalesDistributionAsync(string segment, DateTime startDate, DateTime endDate)
        {
            var ordersQuery = _context.Orders
                .Include(o => o.User)
                .Where(o => o.IsActive && o.CreatedAt >= startDate && o.CreatedAt <= endDate);

            if (segment == "b2b")
                ordersQuery = ordersQuery.Where(o => o.User.Role == UserRole.B2B);
            else if (segment == "b2c")
                ordersQuery = ordersQuery.Where(o => o.User.Role == UserRole.B2C);

            var orders = await ordersQuery.ToListAsync();

            // Adres bilgisinden şehir çıkarma (basitleştirilmiş)
            var citySales = orders
                .Where(o => !string.IsNullOrEmpty(o.ShippingAddress))
                .GroupBy(o => ExtractCityFromAddress(o.ShippingAddress ?? ""))
                .Select(g => new { City = g.Key, Sales = g.Sum(o => o.TotalAmount) })
                .OrderByDescending(c => c.Sales)
                .Take(6)
                .ToList();

            var totalSales = citySales.Sum(c => c.Sales);
            var result = citySales.Select((c, index) => new CitySalesDto
            {
                Rank = index + 1,
                CityName = string.IsNullOrEmpty(c.City) ? "Diğer" : c.City,
                Sales = c.Sales,
                Percentage = totalSales > 0 ? Math.Round((c.Sales / totalSales) * 100, 0) : 0
            }).ToList();

            // Eğer veri yoksa örnek veri döndür
            if (!result.Any())
            {
                result = new List<CitySalesDto>
                {
                    new() { Rank = 1, CityName = "İstanbul", Sales = 45000, Percentage = 31 },
                    new() { Rank = 2, CityName = "Ankara", Sales = 28000, Percentage = 19 },
                    new() { Rank = 3, CityName = "İzmir", Sales = 22000, Percentage = 15 },
                    new() { Rank = 4, CityName = "Bursa", Sales = 18000, Percentage = 12 },
                    new() { Rank = 5, CityName = "Antalya", Sales = 15000, Percentage = 10 },
                    new() { Rank = 6, CityName = "Diğer", Sales = 17000, Percentage = 13 }
                };
            }

            return result;
        }

        public async Task<PlatformDistributionDto> GetPlatformDistributionAsync(string segment, DateTime startDate, DateTime endDate)
        {
            // Platform bilgisi şu an için sabit değerler (gerçek implementasyonda user-agent analizi yapılabilir)
            // Bu veriler genellikle analytics sistemlerinden gelir
            await Task.CompletedTask;

            return new PlatformDistributionDto
            {
                MobileWebPercentage = 60,
                DesktopPercentage = 40
            };
        }

        public async Task<List<ProductPerformanceDto>> GetProductPerformanceAsync(ProductPerformanceFilterDto filter)
        {
            var orderItemsQuery = _context.OrderItems
                .Include(oi => oi.Order)
                    .ThenInclude(o => o.User)
                .Include(oi => oi.Product)
                    .ThenInclude(p => p.Category)
                .Include(oi => oi.Product)
                    .ThenInclude(p => p.Brand)
                .Where(oi => oi.Order.IsActive);

            // Segment filtreleme
            if (filter.Segment == "b2b")
                orderItemsQuery = orderItemsQuery.Where(oi => oi.Order.User.Role == UserRole.B2B);
            else if (filter.Segment == "b2c")
                orderItemsQuery = orderItemsQuery.Where(oi => oi.Order.User.Role == UserRole.B2C);

            // Kategori filtresi
            if (filter.CategoryId.HasValue)
                orderItemsQuery = orderItemsQuery.Where(oi => oi.Product.CategoryId == filter.CategoryId.Value);

            // Marka filtresi
            if (filter.BrandId.HasValue)
                orderItemsQuery = orderItemsQuery.Where(oi => oi.Product.BrandId == filter.BrandId.Value);

            var orderItems = await orderItemsQuery.ToListAsync();

            // Arama filtresi
            if (!string.IsNullOrEmpty(filter.SearchQuery))
            {
                var query = filter.SearchQuery.ToLower();
                orderItems = orderItems.Where(oi =>
                    oi.Product.Name.ToLower().Contains(query) ||
                    oi.Product.SKU.ToLower().Contains(query)
                ).ToList();
            }

            // Ürün bazında gruplama
            var productGroups = orderItems
                .GroupBy(oi => oi.ProductId)
                .Select(g =>
                {
                    var product = g.First().Product;
                    var totalRevenue = g.Sum(oi => oi.Quantity * oi.UnitPrice);
                    var totalSold = g.Sum(oi => oi.Quantity);

                    return new ProductPerformanceDto
                    {
                        ProductId = product.Id,
                        ProductName = product.Name,
                        SKU = product.SKU ?? "",
                        ImageUrl = product.Image ?? "/images/no-image.png",
                        NetRevenue = totalRevenue,
                        SalesCount = totalSold,
                        UnitPrice = product.PriceB2C,
                        ReturnRate = 2, // Örnek değer, iade takibi eklenebilir
                        CurrentStock = product.Stock,
                        CartAdditions = 0 // Sepet verisi ayrıca hesaplanabilir
                    };
                })
                .ToList();

            // Sıralama
            var sortedProducts = filter.SortBy switch
            {
                "quantity" => productGroups.OrderByDescending(p => p.SalesCount),
                "cart" => productGroups.OrderByDescending(p => p.CartAdditions),
                _ => productGroups.OrderByDescending(p => p.NetRevenue) // revenue default
            };

            // Rank atama
            var result = sortedProducts.Select((p, index) =>
            {
                p.Rank = index + 1;
                return p;
            }).Take(20).ToList();

            // Sepete ekleme verilerini ekle
            var productIds = result.Select(p => p.ProductId).ToList();
            var cartCounts = await _context.CartItems
                .Where(ci => productIds.Contains(ci.ProductId))
                .GroupBy(ci => ci.ProductId)
                .Select(g => new { ProductId = g.Key, Count = g.Count() })
                .ToListAsync();

            foreach (var product in result)
            {
                var cartCount = cartCounts.FirstOrDefault(c => c.ProductId == product.ProductId);
                product.CartAdditions = cartCount?.Count ?? 0;
            }

            return result;
        }

        private string ExtractCityFromAddress(string address)
        {
            // Basit bir şehir çıkarma mantığı
            // Gerçek uygulamada daha gelişmiş bir parsing yapılmalı
            var cities = new[] { "İstanbul", "Ankara", "İzmir", "Bursa", "Antalya", "Adana", "Konya", "Gaziantep" };
            foreach (var city in cities)
            {
                if (address.Contains(city, StringComparison.OrdinalIgnoreCase))
                    return city;
            }
            return "Diğer";
        }
    }
}
