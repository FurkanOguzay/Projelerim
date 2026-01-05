using Microsoft.EntityFrameworkCore;
using UpexTech.Data;
using UpexTech.Entity;

namespace UpexTech.Business.Services
{
    public interface IBrowsingHistoryService
    {
        Task RecordViewAsync(int userId, int productId);
        Task<IEnumerable<BrowsingHistory>> GetUserBrowsingHistoryAsync(int userId, int limit = 20);
        Task ClearHistoryAsync(int userId);
    }

    public class BrowsingHistoryService : IBrowsingHistoryService
    {
        private readonly UpexTechDbContext _context;

        public BrowsingHistoryService(UpexTechDbContext context)
        {
            _context = context;
        }

        public async Task RecordViewAsync(int userId, int productId)
        {
            // Zaten bu ürün için kayıt var mı kontrol et
            var existingEntry = await _context.BrowsingHistories
                .FirstOrDefaultAsync(bh => bh.UserId == userId && bh.ProductId == productId);

            if (existingEntry != null)
            {
                // Varsa sadece tarihi güncelle
                existingEntry.ViewedAt = DateTime.Now;
                existingEntry.UpdatedAt = DateTime.Now;
            }
            else
            {
                // Yoksa yeni kayıt oluştur
                var newEntry = new BrowsingHistory
                {
                    UserId = userId,
                    ProductId = productId,
                    ViewedAt = DateTime.Now,
                    CreatedAt = DateTime.Now,
                    IsActive = true
                };
                await _context.BrowsingHistories.AddAsync(newEntry);
            }

            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<BrowsingHistory>> GetUserBrowsingHistoryAsync(int userId, int limit = 20)
        {
            return await _context.BrowsingHistories
                .Include(bh => bh.Product)
                .Where(bh => bh.UserId == userId && bh.IsActive && bh.Product.IsActive)
                .OrderByDescending(bh => bh.ViewedAt)
                .Take(limit)
                .ToListAsync();
        }

        public async Task ClearHistoryAsync(int userId)
        {
            var entries = await _context.BrowsingHistories
                .Where(bh => bh.UserId == userId)
                .ToListAsync();

            foreach (var entry in entries)
            {
                entry.IsActive = false;
                entry.UpdatedAt = DateTime.Now;
            }

            await _context.SaveChangesAsync();
        }
    }
}
