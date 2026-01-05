using Microsoft.AspNetCore.Mvc;
using UpexTech.Admin.Models;
using UpexTech.Business.Services;

namespace UpexTech.Admin.Controllers
{
    public class DashboardController : AdminBaseController
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly IBrandService _brandService;
        private readonly IUserService _userService;
        private readonly IOrderService _orderService;

        public DashboardController(
            IProductService productService,
            ICategoryService categoryService,
            IBrandService brandService,
            IUserService userService,
            IOrderService orderService)
        {
            _productService = productService;
            _categoryService = categoryService;
            _brandService = brandService;
            _userService = userService;
            _orderService = orderService;
        }

        public async Task<IActionResult> Index()
        {
            var products = await _productService.GetAllProductsAsync();
            var categories = await _categoryService.GetAllCategoriesAsync();
            var brands = await _brandService.GetAllBrandsAsync();
            var users = await _userService.GetAllUsersAsync();
            var pendingDealers = await _userService.GetPendingDealersAsync();

            // Haftalık satış verilerini al
            var weeklySalesData = await _orderService.GetWeeklySalesDataAsync();

            // En çok satan kategorileri al
            var topCategories = await _orderService.GetTopSellingCategoriesAsync(5);
            var colors = new[] { "#3b82f6", "#10b981", "#f59e0b", "#8b5cf6", "#6b7280" };
            var categorySales = topCategories.Select((kvp, index) => new CategorySalesDto
            {
                CategoryName = kvp.Key,
                Percentage = kvp.Value,
                Color = colors[Math.Min(index, colors.Length - 1)]
            }).ToList();

            // Son siparişleri al
            var recentOrders = await _orderService.GetRecentOrdersAsync(5);

            // Düşük stoklu ürünleri al
            var lowStockProducts = await _productService.GetCriticalStockProductsAsync();
            var lowStockDtos = lowStockProducts.Take(5).Select(p => new LowStockProductDto
            {
                ProductName = p.Name,
                Stock = p.Stock
            }).ToList();

            var viewModel = new DashboardViewModel
            {
                TotalProducts = products.Count(),
                TotalCategories = categories.Count(),
                TotalBrands = brands.Count(),
                TotalUsers = users.Count(),
                PendingDealers = pendingDealers.Count(),
                PendingOrders = await _orderService.GetPendingOrdersCountAsync(),
                ReturnRequests = await _orderService.GetReturnRequestsCountAsync(),
                DailyRevenue = await _orderService.GetDailyRevenueAsync(),
                B2BRevenue = await _orderService.GetB2BRevenueAsync(),
                B2CRevenue = await _orderService.GetB2CRevenueAsync(),
                WeeklySalesB2B = (decimal[])weeklySalesData["b2b"],
                WeeklySalesB2C = (decimal[])weeklySalesData["b2c"],
                WeeklySalesLabels = (string[])weeklySalesData["labels"],
                TopCategories = categorySales,
                RecentOrders = recentOrders,
                LowStockProducts = lowStockDtos
            };

            return View(viewModel);
        }
    }
}
