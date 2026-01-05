namespace UpexTech.Admin.Models
{
    public class CategorySalesDto
    {
        public string CategoryName { get; set; } = string.Empty;
        public int Percentage { get; set; }
        public string Color { get; set; } = string.Empty;
    }

    public class LowStockProductDto
    {
        public string ProductName { get; set; } = string.Empty;
        public int Stock { get; set; }
    }

    public class DashboardViewModel
    {
        public int TotalProducts { get; set; }
        public int TotalCategories { get; set; }
        public int TotalBrands { get; set; }
        public int TotalUsers { get; set; }
        public int PendingDealers { get; set; }
        public int PendingOrders { get; set; }
        public int ReturnRequests { get; set; }
        public decimal DailyRevenue { get; set; }
        public decimal B2BRevenue { get; set; }
        public decimal B2CRevenue { get; set; }

        // Haftalık Satış Grafiği Verileri
        public decimal[] WeeklySalesB2B { get; set; } = new decimal[7];
        public decimal[] WeeklySalesB2C { get; set; } = new decimal[7];
        public string[] WeeklySalesLabels { get; set; } = new string[7];

        // Dashboard Verileri
        public IEnumerable<CategorySalesDto> TopCategories { get; set; } = new List<CategorySalesDto>();
        public IEnumerable<UpexTech.Entity.Order> RecentOrders { get; set; } = new List<UpexTech.Entity.Order>();
        public IEnumerable<LowStockProductDto> LowStockProducts { get; set; } = new List<LowStockProductDto>();
    }
}
