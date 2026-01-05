using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using UpexTech.Business.Services;
using UpexTech.Entity;

namespace UpexTech.Web.Controllers
{
    [Authorize]
    public class CheckoutController : Controller
    {
        private readonly ICartService _cartService;
        private readonly IOrderService _orderService;
        private readonly IUserService _userService;
        private readonly IPriceListService _priceListService;

        public CheckoutController(
            ICartService cartService, 
            IOrderService orderService,
            IUserService userService,
            IPriceListService priceListService)
        {
            _cartService = cartService;
            _orderService = orderService;
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

            // Sepet boşsa sepet sayfasına yönlendir
            if (cart == null || !cart.Items.Any())
            {
                TempData["Warning"] = "Sepetinizde ürün bulunmamaktadır.";
                return RedirectToAction("Index", "Cart");
            }

            ViewBag.IsB2B = IsB2B();
            ViewBag.UserPriceList = await GetUserPriceListAsync();
            return View(cart);
        }

        [HttpPost]
        public async Task<IActionResult> PlaceOrder([FromForm] CheckoutRequest request)
        {
            try
            {
                var userId = GetUserId();

                // Adres bilgilerini birleştir
                var shippingAddress = $"{request.FirstName} {request.LastName}\n" +
                                    $"Tel: {request.Phone}\n" +
                                    $"Email: {request.Email}\n" +
                                    $"{request.Address}\n" +
                                    $"{request.District}, {request.City} {request.PostalCode}";

                // Sipariş oluştur
                var order = await _orderService.CreateOrderFromCartAsync(userId, shippingAddress, request.OrderNotes);

                if (order == null)
                {
                    return Json(new { success = false, message = "Sepetinizde ürün bulunmamaktadır." });
                }

                return Json(new
                {
                    success = true,
                    message = "Siparişiniz başarıyla oluşturuldu!",
                    orderNumber = order.OrderNumber,
                    orderId = order.Id
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Sipariş oluşturulurken bir hata oluştu: " + ex.Message });
            }
        }
    }

    public class CheckoutRequest
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string District { get; set; } = string.Empty;
        public string PostalCode { get; set; } = string.Empty;
        public string? OrderNotes { get; set; }
    }
}
