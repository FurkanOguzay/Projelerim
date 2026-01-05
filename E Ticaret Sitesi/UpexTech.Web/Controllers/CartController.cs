using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using UpexTech.Business.Services;
using UpexTech.Entity;

namespace UpexTech.Web.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private readonly ICartService _cartService;
        private readonly IProductService _productService;
        private readonly IUserService _userService;
        private readonly IPriceListService _priceListService;

        public CartController(
            ICartService cartService, 
            IProductService productService,
            IUserService userService,
            IPriceListService priceListService)
        {
            _cartService = cartService;
            _productService = productService;
            _userService = userService;
            _priceListService = priceListService;
        }

        private int GetUserId() => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        private bool IsB2B() => User.FindFirst(ClaimTypes.Role)?.Value == UserRole.B2B.ToString();

        /// <summary>
        /// Kullanıcının PriceList bilgisini getirir
        /// </summary>
        private async Task<PriceList?> GetUserPriceListAsync()
        {
            var userId = GetUserId();
            var user = await _userService.GetByIdAsync(userId);
            if (user?.PriceListId != null)
            {
                return await _priceListService.GetPriceListByIdAsync(user.PriceListId.Value);
            }
            return null;
        }

        public async Task<IActionResult> Index()
        {
            var userId = GetUserId();
            var cart = await _cartService.GetCartByUserIdAsync(userId);
            var isB2B = IsB2B();
            var priceList = await GetUserPriceListAsync();

            ViewBag.IsB2B = isB2B;
            ViewBag.UserPriceList = priceList;
            return View(cart);
        }

        [HttpPost]
        public async Task<IActionResult> Add(int productId, int quantity = 1)
        {
            var userId = GetUserId();
            var isB2B = IsB2B();
            var priceList = await GetUserPriceListAsync();

            var cartItem = await _cartService.AddToCartAsync(userId, productId, quantity, isB2B, priceList);
            
            if (cartItem == null)
            {
                return Json(new { success = false, message = "Ürün sepete eklenemedi." });
            }

            var cartCount = await _cartService.GetCartItemCountAsync(userId);

            return Json(new { 
                success = true, 
                message = "Ürün sepete eklendi.",
                cartCount = cartCount,
                itemQuantity = cartItem.Quantity
            });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateQuantity(int productId, int quantity)
        {
            var userId = GetUserId();
            var product = await _productService.GetProductByIdAsync(productId);
            var priceList = await GetUserPriceListAsync();
            
            if (product == null)
            {
                return Json(new { success = false, message = "Ürün bulunamadı." });
            }

            // Stok kontrolü
            if (quantity > product.Stock)
            {
                quantity = product.Stock;
            }

            var cartItem = await _cartService.UpdateQuantityAsync(userId, productId, quantity);
            
            if (cartItem == null || !cartItem.IsActive)
            {
                // Item kaldırıldı
                var count = await _cartService.GetCartItemCountAsync(userId);
                var total = await _cartService.GetCartTotalAsync(userId, IsB2B(), priceList);
                return Json(new { 
                    success = true, 
                    removed = true,
                    cartCount = count,
                    cartTotal = total
                });
            }

            var cartCount = await _cartService.GetCartItemCountAsync(userId);
            var cartTotal = await _cartService.GetCartTotalAsync(userId, IsB2B(), priceList);
            var itemTotal = cartItem.Quantity * cartItem.UnitPrice;

            return Json(new { 
                success = true, 
                quantity = cartItem.Quantity,
                itemTotal = itemTotal,
                cartCount = cartCount,
                cartTotal = cartTotal,
                maxStock = product.Stock
            });
        }

        [HttpPost]
        public async Task<IActionResult> Remove(int productId)
        {
            var userId = GetUserId();
            var result = await _cartService.RemoveFromCartAsync(userId, productId);
            var priceList = await GetUserPriceListAsync();

            if (!result)
            {
                return Json(new { success = false, message = "Ürün sepetten kaldırılamadı." });
            }

            var cartCount = await _cartService.GetCartItemCountAsync(userId);
            var cartTotal = await _cartService.GetCartTotalAsync(userId, IsB2B(), priceList);

            return Json(new { 
                success = true, 
                message = "Ürün sepetten kaldırıldı.",
                cartCount = cartCount,
                cartTotal = cartTotal
            });
        }

        [HttpPost]
        public async Task<IActionResult> Clear()
        {
            var userId = GetUserId();
            await _cartService.ClearCartAsync(userId);

            return Json(new { success = true, message = "Sepet temizlendi." });
        }

        [HttpGet]
        public async Task<IActionResult> GetCount()
        {
            try
            {
                var userId = GetUserId();
                var count = await _cartService.GetCartItemCountAsync(userId);
                return Json(new { count });
            }
            catch
            {
                return Json(new { count = 0 });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetStock(int productId)
        {
            var product = await _productService.GetProductByIdAsync(productId);
            if (product == null)
            {
                return Json(new { success = false });
            }

            return Json(new { success = true, stock = product.Stock });
        }

        [HttpGet]
        public async Task<IActionResult> GetPreview()
        {
            try
            {
                var userId = GetUserId();
                var cart = await _cartService.GetCartByUserIdAsync(userId);
                var isB2B = IsB2B();
                var priceList = await GetUserPriceListAsync();

                if (cart == null || !cart.Items.Any())
                {
                    return Json(new { items = Array.Empty<object>(), total = 0m });
                }

                var items = cart.Items.Where(i => i.IsActive).Select(i => new
                {
                    productId = i.ProductId,
                    name = i.Product?.Name ?? "Ürün",
                    image = i.Product?.Image != null ? $"/images/{i.Product.Image}" : "/images/placeholder.png",
                    quantity = i.Quantity,
                    price = CalculatePrice(i.Product!, isB2B, priceList)
                }).ToList();

                var total = items.Sum(i => i.quantity * i.price);

                return Json(new { items, total });
            }
            catch
            {
                return Json(new { items = Array.Empty<object>(), total = 0m });
            }
        }

        /// <summary>
        /// Ürün fiyatını PriceList'e göre hesaplar
        /// </summary>
        private decimal CalculatePrice(Product product, bool isB2B, PriceList? priceList)
        {
            decimal defaultPrice = isB2B ? product.PriceB2B : product.PriceB2C;
            
            if (priceList == null)
            {
                return defaultPrice;
            }

            decimal basePrice = product.PurchasePrice > 0 ? product.PurchasePrice : defaultPrice;
            decimal calculatedPrice = basePrice * priceList.Factor;
            
            calculatedPrice = priceList.Rounding switch
            {
                RoundingMethod.Ending90 => Math.Floor(calculatedPrice) + 0.90m,
                RoundingMethod.Ending99 => Math.Floor(calculatedPrice) + 0.99m,
                RoundingMethod.NearestFive => Math.Round(calculatedPrice / 5) * 5,
                _ => Math.Round(calculatedPrice, 2)
            };
            
            return calculatedPrice > 0 ? calculatedPrice : defaultPrice;
        }
    }
}
