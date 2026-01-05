using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using UpexTech.Business.Services;
using UpexTech.Web.Models;

namespace UpexTech.Web.Controllers
{
    [Authorize]
    public class FavoritesController : Controller
    {
        private readonly IFavoriteService _favoriteService;

        public FavoritesController(IFavoriteService favoriteService)
        {
            _favoriteService = favoriteService;
        }

        private int GetUserId()
        {
            return int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        }

        // Favoriler Sayfası (Favoriler + Koleksiyonlar sekmeli)
        public async Task<IActionResult> Index(string tab = "favorites")
        {
            var userId = GetUserId();
            var favorites = await _favoriteService.GetUserFavoritesAsync(userId);
            var collections = await _favoriteService.GetUserCollectionsAsync(userId);

            var viewModel = new FavoritesViewModel
            {
                Favorites = favorites.ToList(),
                Collections = collections.ToList(),
                ActiveTab = tab
            };

            return View(viewModel);
        }

        #region Favori İşlemleri

        [HttpPost]
        public async Task<IActionResult> Toggle(int productId)
        {
            var userId = GetUserId();
            await _favoriteService.ToggleFavoriteAsync(userId, productId);
            var isFavorite = await _favoriteService.IsFavoriteAsync(userId, productId);
            
            return Json(new { success = true, isFavorite });
        }

        [HttpGet]
        public async Task<IActionResult> Check(int productId)
        {
            var userId = GetUserId();
            var isFavorite = await _favoriteService.IsFavoriteAsync(userId, productId);
            
            return Json(new { isFavorite });
        }

        [HttpPost]
        public async Task<IActionResult> Remove(int productId)
        {
            var userId = GetUserId();
            await _favoriteService.RemoveFavoriteAsync(userId, productId);
            
            TempData["Success"] = "Ürün favorilerden kaldırıldı.";
            return RedirectToAction("Index");
        }

        #endregion

        #region Koleksiyon İşlemleri

        // Koleksiyon Detay Sayfası
        public async Task<IActionResult> Collection(int id)
        {
            var userId = GetUserId();
            var collection = await _favoriteService.GetCollectionByIdAsync(id, userId);

            if (collection == null)
            {
                TempData["Error"] = "Koleksiyon bulunamadı.";
                return RedirectToAction("Index", new { tab = "collections" });
            }

            return View(collection);
        }

        // Yeni Koleksiyon Oluştur
        [HttpPost]
        public async Task<IActionResult> CreateCollection(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                TempData["Error"] = "Koleksiyon adı boş olamaz.";
                return RedirectToAction("Index", new { tab = "collections" });
            }

            var userId = GetUserId();
            await _favoriteService.CreateCollectionAsync(userId, name);

            TempData["Success"] = "Koleksiyon başarıyla oluşturuldu.";
            return RedirectToAction("Index", new { tab = "collections" });
        }

        // Koleksiyon Adını Güncelle
        [HttpPost]
        public async Task<IActionResult> UpdateCollection(int id, string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return Json(new { success = false, message = "Koleksiyon adı boş olamaz." });
            }

            var userId = GetUserId();
            var result = await _favoriteService.UpdateCollectionAsync(id, userId, name);

            if (result)
            {
                return Json(new { success = true, message = "Koleksiyon güncellendi." });
            }

            return Json(new { success = false, message = "Koleksiyon güncellenemedi." });
        }

        // Koleksiyon Sil
        [HttpPost]
        public async Task<IActionResult> DeleteCollection(int id)
        {
            var userId = GetUserId();
            var result = await _favoriteService.DeleteCollectionAsync(id, userId);

            // AJAX isteği mi kontrol et
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest" || 
                Request.Headers["Accept"].ToString().Contains("application/json") ||
                Request.ContentType?.Contains("application/x-www-form-urlencoded") == true)
            {
                if (result)
                {
                    return Json(new { success = true, message = "Koleksiyon silindi." });
                }
                return Json(new { success = false, message = "Koleksiyon silinemedi." });
            }

            // Normal form submit için
            if (result)
            {
                TempData["Success"] = "Koleksiyon silindi.";
            }
            else
            {
                TempData["Error"] = "Koleksiyon silinemedi.";
            }

            return RedirectToAction("Index", new { tab = "collections" });
        }

        #endregion

        #region Koleksiyona Ürün Ekleme/Çıkarma

        // Ürünü koleksiyona ekle
        [HttpPost]
        public async Task<IActionResult> AddToCollection(int collectionId, int productId)
        {
            var userId = GetUserId();
            
            // Önce favoride olduğundan emin ol
            var favorite = await _favoriteService.GetFavoriteAsync(userId, productId);
            if (favorite == null)
            {
                return Json(new { success = false, message = "Ürün favorilerinizde değil." });
            }

            var result = await _favoriteService.AddToCollectionAsync(collectionId, favorite.Id, userId);

            if (result)
            {
                return Json(new { success = true, message = "Ürün koleksiyona eklendi." });
            }

            return Json(new { success = false, message = "Ürün koleksiyona eklenemedi." });
        }

        // Ürünü koleksiyondan çıkar
        [HttpPost]
        public async Task<IActionResult> RemoveFromCollection(int collectionId, int productId)
        {
            var userId = GetUserId();
            
            var favorite = await _favoriteService.GetFavoriteAsync(userId, productId);
            if (favorite == null)
            {
                return Json(new { success = false, message = "Ürün favorilerinizde değil." });
            }

            var result = await _favoriteService.RemoveFromCollectionAsync(collectionId, favorite.Id, userId);

            if (result)
            {
                return Json(new { success = true, message = "Ürün koleksiyondan çıkarıldı." });
            }

            return Json(new { success = false, message = "Ürün koleksiyondan çıkarılamadı." });
        }

        // Ürünün hangi koleksiyonlarda olduğunu getir
        [HttpGet]
        public async Task<IActionResult> GetProductCollections(int productId)
        {
            var userId = GetUserId();
            
            var favorite = await _favoriteService.GetFavoriteAsync(userId, productId);
            if (favorite == null)
            {
                return Json(new { success = false, collectionIds = new List<int>() });
            }

            var collectionIds = await _favoriteService.GetFavoriteCollectionIdsAsync(favorite.Id);
            var collections = await _favoriteService.GetUserCollectionsAsync(userId);

            return Json(new { 
                success = true, 
                collectionIds = collectionIds,
                collections = collections.Select(c => new { c.Id, c.Name, itemCount = c.Items.Count(i => i.IsActive) })
            });
        }

        // Ürünün koleksiyon üyeliklerini toplu güncelle
        [HttpPost]
        public async Task<IActionResult> UpdateProductCollections(int productId, [FromBody] List<int> collectionIds)
        {
            var userId = GetUserId();
            
            var favorite = await _favoriteService.GetFavoriteAsync(userId, productId);
            if (favorite == null)
            {
                return Json(new { success = false, message = "Ürün favorilerinizde değil." });
            }

            // Mevcut koleksiyonları al
            var currentCollectionIds = await _favoriteService.GetFavoriteCollectionIdsAsync(favorite.Id);
            var allCollections = await _favoriteService.GetUserCollectionsAsync(userId);

            // Çıkarılacaklar
            var toRemove = currentCollectionIds.Except(collectionIds ?? new List<int>());
            foreach (var collId in toRemove)
            {
                await _favoriteService.RemoveFromCollectionAsync(collId, favorite.Id, userId);
            }

            // Eklenecekler
            var toAdd = (collectionIds ?? new List<int>()).Except(currentCollectionIds);
            foreach (var collId in toAdd)
            {
                await _favoriteService.AddToCollectionAsync(collId, favorite.Id, userId);
            }

            return Json(new { success = true, message = "Koleksiyonlar güncellendi." });
        }

        #endregion
    }
}
