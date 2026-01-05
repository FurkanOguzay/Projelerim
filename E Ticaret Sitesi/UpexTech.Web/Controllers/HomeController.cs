using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using UpexTech.Business.Services;
using UpexTech.Entity;
using UpexTech.Web.Models;

namespace UpexTech.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly IUserService _userService;
        private readonly IPriceListService _priceListService;

        public HomeController(
            IProductService productService, 
            ICategoryService categoryService,
            IUserService userService,
            IPriceListService priceListService)
        {
            _productService = productService;
            _categoryService = categoryService;
            _userService = userService;
            _priceListService = priceListService;
        }

        public async Task<IActionResult> Index()
        {
            var popularProducts = await _productService.GetPopularProductsAsync();
            var immediateDeliveryProducts = await _productService.GetImmediateDeliveryProductsAsync();
            var categories = await _categoryService.GetAllCategoriesWithBrandsAsync();

            var isLoggedIn = User.Identity?.IsAuthenticated ?? false;
            UserRole? userRole = null;
            PriceList? userPriceList = null;

            if (isLoggedIn)
            {
                var roleStr = User.FindFirst(ClaimTypes.Role)?.Value;
                if (Enum.TryParse<UserRole>(roleStr, out var role))
                {
                    userRole = role;
                }

                // Kullanıcının PriceList bilgisini al
                var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (int.TryParse(userIdStr, out var userId))
                {
                    var user = await _userService.GetByIdAsync(userId);
                    if (user?.PriceListId != null)
                    {
                        userPriceList = await _priceListService.GetPriceListByIdAsync(user.PriceListId.Value);
                    }
                }
            }

            var viewModel = new HomeViewModel
            {
                PopularProducts = popularProducts.ToList(),
                ImmediateDeliveryProducts = immediateDeliveryProducts.ToList(),
                Categories = categories.ToList(),
                IsLoggedIn = isLoggedIn,
                UserRole = userRole,
                UserPriceList = userPriceList
            };

            return View(viewModel);
        }

        /// <summary>
        /// Ürün için müşteriye özel fiyat hesaplar
        /// </summary>
        public decimal CalculateProductPrice(Product product, bool isB2B, PriceList? priceList)
        {
            // PriceList varsa, PurchasePrice üzerinden hesapla
            if (priceList != null)
            {
                return _priceListService.CalculatePrice(product.PurchasePrice, priceList);
            }

            // PriceList yoksa varsayılan fiyatları kullan
            return isB2B ? product.PriceB2B : product.PriceB2C;
        }

        public IActionResult Error()
        {
            return View();
        }

        public IActionResult About()
        {
            return View();
        }
    }
}
