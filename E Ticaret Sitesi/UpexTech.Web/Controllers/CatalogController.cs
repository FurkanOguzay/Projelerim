using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using UpexTech.Business.Services;
using UpexTech.Entity;

namespace UpexTech.Web.Controllers
{
    public class CatalogController : Controller
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly IBrandService _brandService;
        private readonly IFavoriteService _favoriteService;
        private readonly IStockAlertService _stockAlertService;
        private readonly IReviewService _reviewService;
        private readonly IBrowsingHistoryService _browsingHistoryService;
        private readonly IUserService _userService;
        private readonly IPriceListService _priceListService;

        public CatalogController(
            IProductService productService, 
            ICategoryService categoryService,
            IBrandService brandService,
            IFavoriteService favoriteService,
            IStockAlertService stockAlertService,
            IReviewService reviewService,
            IBrowsingHistoryService browsingHistoryService,
            IUserService userService,
            IPriceListService priceListService)
        {
            _productService = productService;
            _categoryService = categoryService;
            _brandService = brandService;
            _favoriteService = favoriteService;
            _stockAlertService = stockAlertService;
            _reviewService = reviewService;
            _browsingHistoryService = browsingHistoryService;
            _userService = userService;
            _priceListService = priceListService;
        }

        public async Task<IActionResult> Index(
            int? categoryId, 
            int? brandId, 
            string? search, 
            string? sort,
            string? colors,      // Virgülle ayrılmış renk filtreleri
            string? materials,   // Virgülle ayrılmış malzeme filtreleri
            decimal? minPrice,   // Minimum fiyat
            decimal? maxPrice,   // Maksimum fiyat
            bool? inStockOnly)   // Sadece stokta olanlar
        {
            ViewBag.Categories = await _categoryService.GetAllCategoriesWithBrandsAsync();
            ViewBag.SelectedCategoryId = categoryId;
            ViewBag.SelectedBrandId = brandId;
            ViewBag.SearchTerm = search;
            ViewBag.CurrentSort = sort;
            ViewBag.SelectedColors = colors;
            ViewBag.SelectedMaterials = materials;
            ViewBag.MinPrice = minPrice;
            ViewBag.MaxPrice = maxPrice;
            ViewBag.InStockOnly = inStockOnly ?? false;
            ViewBag.IsLoggedIn = User.Identity?.IsAuthenticated ?? false;
            PriceList? userPriceList = null;
            
            if (ViewBag.IsLoggedIn)
            {
                var roleStr = User.FindFirst(ClaimTypes.Role)?.Value;
                ViewBag.UserRole = Enum.TryParse<UserRole>(roleStr, out var role) ? role : UserRole.B2C;
                
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
            
            ViewBag.UserPriceList = userPriceList;

            // İlk sayfa ürünlerini getir
            var (products, hasMore) = await _productService.GetProductsInfiniteAsync(1, 8, categoryId, brandId, search);
            
            // Ek filtreleri uygula
            var productList = products.ToList();
            
            // Renk filtresi
            if (!string.IsNullOrEmpty(colors))
            {
                var colorList = colors.Split(',', StringSplitOptions.RemoveEmptyEntries);
                productList = productList.Where(p => !string.IsNullOrEmpty(p.Color) && colorList.Contains(p.Color)).ToList();
            }
            
            // Malzeme filtresi
            if (!string.IsNullOrEmpty(materials))
            {
                var materialList = materials.Split(',', StringSplitOptions.RemoveEmptyEntries);
                productList = productList.Where(p => !string.IsNullOrEmpty(p.Material) && materialList.Contains(p.Material)).ToList();
            }
            
            // Fiyat aralığı filtresi
            if (minPrice.HasValue)
            {
                productList = productList.Where(p => p.PriceB2C >= minPrice.Value).ToList();
            }
            if (maxPrice.HasValue && maxPrice.Value > 0)
            {
                productList = productList.Where(p => p.PriceB2C <= maxPrice.Value).ToList();
            }
            
            // Stok filtresi
            if (inStockOnly == true)
            {
                productList = productList.Where(p => p.Stock > 0).ToList();
            }
            
            // Sıralama uygula
            switch (sort)
            {
                case "price_asc":
                    productList = productList.OrderBy(p => p.PriceB2C).ToList();
                    break;
                case "price_desc":
                    productList = productList.OrderByDescending(p => p.PriceB2C).ToList();
                    break;
                case "newest":
                    productList = productList.OrderByDescending(p => p.CreatedAt).ToList();
                    break;
                case "rating":
                    productList = productList.OrderByDescending(p => p.Rating).ToList();
                    break;
            }
            
            ViewBag.HasMore = hasMore;

            return View(productList.AsEnumerable());
        }

        [HttpGet]
        public async Task<IActionResult> LoadMore(int page, int? categoryId, int? brandId, string? search)
        {
            var (products, hasMore) = await _productService.GetProductsInfiniteAsync(page, 8, categoryId, brandId, search);
            
            var isLoggedIn = User.Identity?.IsAuthenticated ?? false;
            var isB2B = false;
            var userId = 0;

            if (isLoggedIn)
            {
                var roleStr = User.FindFirst(ClaimTypes.Role)?.Value;
                isB2B = roleStr == UserRole.B2B.ToString();
                userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            }

            var productList = new List<object>();
            foreach (var product in products)
            {
                var isFavorite = isLoggedIn && userId > 0 
                    ? await _favoriteService.IsFavoriteAsync(userId, product.Id) 
                    : false;

                productList.Add(new
                {
                    id = product.Id,
                    name = product.Name,
                    description = product.Description,
                    image = product.Image,
                    priceB2C = product.PriceB2C,
                    priceB2B = product.PriceB2B,
                    rating = product.Rating,
                    reviewCount = product.ReviewCount,
                    stock = product.Stock,
                    brandName = product.Brand?.Name,
                    categoryName = product.Category?.Name,
                    isFavorite = isFavorite
                });
            }

            return Json(new { 
                products = productList, 
                hasMore = hasMore,
                isLoggedIn = isLoggedIn,
                isB2B = isB2B
            });
        }

        public async Task<IActionResult> Details(int id)
        {
            var product = await _productService.GetProductWithDetailsAsync(id);
            if (product == null)
                return NotFound();

            ViewBag.IsLoggedIn = User.Identity?.IsAuthenticated ?? false;
            PriceList? userPriceList = null;
            
            if (ViewBag.IsLoggedIn)
            {
                var roleStr = User.FindFirst(ClaimTypes.Role)?.Value;
                ViewBag.UserRole = Enum.TryParse<UserRole>(roleStr, out var role) ? role : UserRole.B2C;
                ViewBag.UserEmail = User.FindFirst(ClaimTypes.Email)?.Value;
                
                // Check if user has already reviewed this product
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                ViewBag.UserId = userId;
                ViewBag.HasUserReviewed = await _reviewService.HasUserReviewedProductAsync(userId, id);
                
                // Record this product view in browsing history
                await _browsingHistoryService.RecordViewAsync(userId, id);
                
                // Kullanıcının PriceList bilgisini al
                var user = await _userService.GetByIdAsync(userId);
                if (user?.PriceListId != null)
                {
                    userPriceList = await _priceListService.GetPriceListByIdAsync(user.PriceListId.Value);
                }
            }
            
            ViewBag.UserPriceList = userPriceList;

            // Get related products for cross-sell
            ViewBag.RelatedProducts = await _productService.GetRelatedProductsAsync(id, 6);
            
            // Get reviews for this product
            ViewBag.Reviews = await _reviewService.GetReviewsByProductIdAsync(id);
            var ratingStats = await _reviewService.GetProductRatingStatsAsync(id);
            ViewBag.RatingStats = ratingStats;

            return View(product);
        }

        [HttpGet]
        public async Task<IActionResult> GetVariationInfo(int productId, string variationType, string value)
        {
            if (!Enum.TryParse<VariationType>(variationType, out var type))
                return Json(new { success = false, message = "Geçersiz varyasyon tipi" });

            var variation = await _productService.GetVariationAsync(productId, type, value);
            if (variation == null)
                return Json(new { success = false, message = "Varyasyon bulunamadı" });

            var product = await _productService.GetProductByIdAsync(productId);
            if (product == null)
                return Json(new { success = false, message = "Ürün bulunamadı" });

            var isB2B = false;
            if (User.Identity?.IsAuthenticated == true)
            {
                var roleStr = User.FindFirst(ClaimTypes.Role)?.Value;
                isB2B = roleStr == UserRole.B2B.ToString();
            }

            var basePrice = isB2B ? product.PriceB2B : product.PriceB2C;
            var finalPrice = basePrice + variation.PriceAdjustment;

            return Json(new { 
                success = true, 
                stock = variation.Stock,
                price = finalPrice,
                priceFormatted = finalPrice.ToString("N0") + " ₺",
                sku = variation.SKU,
                imageUrl = variation.ImageUrl
            });
        }

        [HttpPost]
        public async Task<IActionResult> NotifyWhenAvailable([FromBody] NotifyRequest request)
        {
            if (string.IsNullOrEmpty(request.Email))
            {
                // Try to get email from logged-in user
                if (User.Identity?.IsAuthenticated == true)
                {
                    request.Email = User.FindFirst(ClaimTypes.Email)?.Value ?? "";
                }
            }

            if (string.IsNullOrEmpty(request.Email))
                return Json(new { success = false, message = "Email adresi gereklidir" });

            // Check if alert already exists
            var exists = await _stockAlertService.HasAlertAsync(request.ProductId, request.VariationId, request.Email);
            if (exists)
                return Json(new { success = true, message = "Bu ürün için zaten bildirim kaydınız var" });

            int? userId = null;
            if (User.Identity?.IsAuthenticated == true)
            {
                userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                if (userId == 0) userId = null;
            }

            await _stockAlertService.CreateAlertAsync(request.ProductId, request.VariationId, userId, request.Email);

            return Json(new { success = true, message = "Ürün stoğa girdiğinde size bildirim gönderilecek" });
        }

        [HttpGet]
        public async Task<IActionResult> SearchSuggestions(string query)
        {
            if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
            {
                return Json(new { suggestions = new List<object>() });
            }

            var products = await _productService.SearchProductsAsync(query);
            var suggestions = products.Take(8).Select(p => new
            {
                id = p.Id,
                name = p.Name,
                image = p.Image,
                price = p.PriceB2C,
                category = p.Category?.Name,
                brand = p.Brand?.Name
            });

            return Json(new { suggestions });
        }

        [HttpPost]
        [Microsoft.AspNetCore.Authorization.Authorize]
        public async Task<IActionResult> AddReview([FromBody] AddReviewRequest request)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                if (userId == 0)
                    return Json(new { success = false, message = "Yorum yapmak için giriş yapmalısınız" });

                // Validation
                if (request.Rating < 1 || request.Rating > 5)
                    return Json(new { success = false, message = "Puan 1-5 arasında olmalıdır" });

                if (string.IsNullOrWhiteSpace(request.Comment) || request.Comment.Length < 10)
                    return Json(new { success = false, message = "Yorum en az 10 karakter olmalıdır" });

                // Check if already reviewed
                var hasReviewed = await _reviewService.HasUserReviewedProductAsync(userId, request.ProductId);
                if (hasReviewed)
                    return Json(new { success = false, message = "Bu ürün için zaten bir değerlendirme yapmışsınız" });

                var review = new Review
                {
                    ProductId = request.ProductId,
                    UserId = userId,
                    Rating = request.Rating,
                    Title = request.Title,
                    Comment = request.Comment
                };

                await _reviewService.AddReviewAsync(review);

                return Json(new { 
                    success = true, 
                    message = "Değerlendirmeniz başarıyla eklendi!" 
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Bir hata oluştu: " + ex.Message });
            }
        }
    }

    public class NotifyRequest
    {
        public int ProductId { get; set; }
        public int? VariationId { get; set; }
        public string? Email { get; set; }
    }

    public class AddReviewRequest
    {
        public int ProductId { get; set; }
        public int Rating { get; set; }
        public string? Title { get; set; }
        public string Comment { get; set; } = string.Empty;
    }
}
