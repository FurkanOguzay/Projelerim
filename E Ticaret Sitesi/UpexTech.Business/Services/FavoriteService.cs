using Microsoft.EntityFrameworkCore;
using UpexTech.Data;
using UpexTech.Data.Repositories;
using UpexTech.Entity;

namespace UpexTech.Business.Services
{
    public interface IFavoriteService
    {
        // Favori işlemleri
        Task<IEnumerable<Favorite>> GetUserFavoritesAsync(int userId);
        Task<bool> IsFavoriteAsync(int userId, int productId);
        Task AddFavoriteAsync(int userId, int productId);
        Task RemoveFavoriteAsync(int userId, int productId);
        Task ToggleFavoriteAsync(int userId, int productId);
        Task<Favorite?> GetFavoriteAsync(int userId, int productId);
        
        // Koleksiyon işlemleri
        Task<IEnumerable<FavoriteCollection>> GetUserCollectionsAsync(int userId);
        Task<FavoriteCollection?> GetCollectionByIdAsync(int collectionId, int userId);
        Task<FavoriteCollection> CreateCollectionAsync(int userId, string name);
        Task<bool> UpdateCollectionAsync(int collectionId, int userId, string newName);
        Task<bool> DeleteCollectionAsync(int collectionId, int userId);
        
        // Koleksiyona ürün ekleme/çıkarma
        Task<bool> AddToCollectionAsync(int collectionId, int favoriteId, int userId);
        Task<bool> RemoveFromCollectionAsync(int collectionId, int favoriteId, int userId);
        Task<IEnumerable<int>> GetFavoriteCollectionIdsAsync(int favoriteId);
    }

    public class FavoriteService : IFavoriteService
    {
        private readonly UpexTechDbContext _context;

        public FavoriteService(UpexTechDbContext context)
        {
            _context = context;
        }

        #region Favori İşlemleri

        public async Task<IEnumerable<Favorite>> GetUserFavoritesAsync(int userId)
        {
            var favorites = await _context.Favorites
                .Include(f => f.Product)
                    .ThenInclude(p => p.Category)
                .Include(f => f.Product)
                    .ThenInclude(p => p.Brand)
                .Where(f => f.UserId == userId && f.IsActive)
                .ToListAsync();

            // CollectionItems'ı ayrı yükle
            var favoriteIds = favorites.Select(f => f.Id).ToList();
            var collectionItems = await _context.FavoriteCollectionItems
                .Include(ci => ci.Collection)
                .Where(ci => favoriteIds.Contains(ci.FavoriteId) && ci.IsActive)
                .ToListAsync();

            // Her favori için CollectionItems'ı ata
            foreach (var favorite in favorites)
            {
                favorite.CollectionItems = collectionItems.Where(ci => ci.FavoriteId == favorite.Id).ToList();
            }

            return favorites;
        }

        public async Task<bool> IsFavoriteAsync(int userId, int productId)
        {
            return await _context.Favorites
                .AnyAsync(f => f.UserId == userId && f.ProductId == productId && f.IsActive);
        }

        public async Task<Favorite?> GetFavoriteAsync(int userId, int productId)
        {
            return await _context.Favorites
                .FirstOrDefaultAsync(f => f.UserId == userId && f.ProductId == productId && f.IsActive);
        }

        public async Task AddFavoriteAsync(int userId, int productId)
        {
            var existingFavorite = await _context.Favorites
                .FirstOrDefaultAsync(f => f.UserId == userId && f.ProductId == productId);

            if (existingFavorite != null)
            {
                if (!existingFavorite.IsActive)
                {
                    existingFavorite.IsActive = true;
                    existingFavorite.UpdatedAt = DateTime.Now;
                    await _context.SaveChangesAsync();
                }
            }
            else
            {
                var favorite = new Favorite
                {
                    UserId = userId,
                    ProductId = productId,
                    CreatedAt = DateTime.Now,
                    IsActive = true
                };
                _context.Favorites.Add(favorite);
                await _context.SaveChangesAsync();
            }
        }

        public async Task RemoveFavoriteAsync(int userId, int productId)
        {
            var favorite = await _context.Favorites
                .FirstOrDefaultAsync(f => f.UserId == userId && f.ProductId == productId && f.IsActive);

            if (favorite != null)
            {
                favorite.IsActive = false;
                favorite.UpdatedAt = DateTime.Now;
                await _context.SaveChangesAsync();
            }
        }

        public async Task ToggleFavoriteAsync(int userId, int productId)
        {
            if (await IsFavoriteAsync(userId, productId))
            {
                await RemoveFavoriteAsync(userId, productId);
            }
            else
            {
                await AddFavoriteAsync(userId, productId);
            }
        }

        #endregion

        #region Koleksiyon İşlemleri

        public async Task<IEnumerable<FavoriteCollection>> GetUserCollectionsAsync(int userId)
        {
            return await _context.FavoriteCollections
                .Include(c => c.Items)
                    .ThenInclude(i => i.Favorite)
                        .ThenInclude(f => f.Product)
                .Where(c => c.UserId == userId && c.IsActive)
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<FavoriteCollection?> GetCollectionByIdAsync(int collectionId, int userId)
        {
            return await _context.FavoriteCollections
                .Include(c => c.Items)
                    .ThenInclude(i => i.Favorite)
                        .ThenInclude(f => f.Product)
                            .ThenInclude(p => p.Brand)
                .Include(c => c.Items)
                    .ThenInclude(i => i.Favorite)
                        .ThenInclude(f => f.Product)
                            .ThenInclude(p => p.Category)
                .FirstOrDefaultAsync(c => c.Id == collectionId && c.UserId == userId && c.IsActive);
        }

        public async Task<FavoriteCollection> CreateCollectionAsync(int userId, string name)
        {
            var collection = new FavoriteCollection
            {
                UserId = userId,
                Name = name.Trim(),
                CreatedAt = DateTime.Now,
                IsActive = true
            };

            _context.FavoriteCollections.Add(collection);
            await _context.SaveChangesAsync();

            return collection;
        }

        public async Task<bool> UpdateCollectionAsync(int collectionId, int userId, string newName)
        {
            var collection = await _context.FavoriteCollections
                .FirstOrDefaultAsync(c => c.Id == collectionId && c.UserId == userId && c.IsActive);

            if (collection == null) return false;

            collection.Name = newName.Trim();
            collection.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeleteCollectionAsync(int collectionId, int userId)
        {
            var collection = await _context.FavoriteCollections
                .FirstOrDefaultAsync(c => c.Id == collectionId && c.UserId == userId && c.IsActive);

            if (collection == null) return false;

            collection.IsActive = false;
            collection.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();

            return true;
        }

        #endregion

        #region Koleksiyona Ürün Ekleme/Çıkarma

        public async Task<bool> AddToCollectionAsync(int collectionId, int favoriteId, int userId)
        {
            // Koleksiyonun kullanıcıya ait olduğunu kontrol et
            var collection = await _context.FavoriteCollections
                .FirstOrDefaultAsync(c => c.Id == collectionId && c.UserId == userId && c.IsActive);

            if (collection == null) return false;

            // Favorinin kullanıcıya ait olduğunu kontrol et
            var favorite = await _context.Favorites
                .FirstOrDefaultAsync(f => f.Id == favoriteId && f.UserId == userId && f.IsActive);

            if (favorite == null) return false;

            // Zaten eklenmişse kontrol et
            var existingItem = await _context.FavoriteCollectionItems
                .FirstOrDefaultAsync(i => i.CollectionId == collectionId && i.FavoriteId == favoriteId);

            if (existingItem != null)
            {
                if (!existingItem.IsActive)
                {
                    existingItem.IsActive = true;
                    existingItem.UpdatedAt = DateTime.Now;
                    await _context.SaveChangesAsync();
                }
                return true;
            }

            var item = new FavoriteCollectionItem
            {
                CollectionId = collectionId,
                FavoriteId = favoriteId,
                CreatedAt = DateTime.Now,
                IsActive = true
            };

            _context.FavoriteCollectionItems.Add(item);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> RemoveFromCollectionAsync(int collectionId, int favoriteId, int userId)
        {
            var collection = await _context.FavoriteCollections
                .FirstOrDefaultAsync(c => c.Id == collectionId && c.UserId == userId && c.IsActive);

            if (collection == null) return false;

            var item = await _context.FavoriteCollectionItems
                .FirstOrDefaultAsync(i => i.CollectionId == collectionId && i.FavoriteId == favoriteId && i.IsActive);

            if (item == null) return false;

            item.IsActive = false;
            item.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<IEnumerable<int>> GetFavoriteCollectionIdsAsync(int favoriteId)
        {
            return await _context.FavoriteCollectionItems
                .Where(i => i.FavoriteId == favoriteId && i.IsActive)
                .Select(i => i.CollectionId)
                .ToListAsync();
        }

        #endregion
    }
}
