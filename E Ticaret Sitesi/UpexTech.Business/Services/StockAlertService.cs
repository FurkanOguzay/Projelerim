using Microsoft.EntityFrameworkCore;
using UpexTech.Data;
using UpexTech.Entity;

namespace UpexTech.Business.Services
{
    public class StockAlertService : IStockAlertService
    {
        private readonly UpexTechDbContext _context;

        public StockAlertService(UpexTechDbContext context)
        {
            _context = context;
        }

        public async Task<StockAlert> CreateAlertAsync(int productId, int? productVariationId, int? userId, string email)
        {
            var alert = new StockAlert
            {
                ProductId = productId,
                ProductVariationId = productVariationId,
                UserId = userId,
                Email = email,
                IsNotified = false,
                CreatedAt = DateTime.Now
            };

            _context.StockAlerts.Add(alert);
            await _context.SaveChangesAsync();
            return alert;
        }

        public async Task<bool> HasAlertAsync(int productId, int? productVariationId, string email)
        {
            return await _context.StockAlerts
                .AnyAsync(a => a.ProductId == productId && 
                              a.ProductVariationId == productVariationId && 
                              a.Email == email && 
                              !a.IsNotified);
        }

        public async Task<IEnumerable<StockAlert>> GetAlertsForProductAsync(int productId)
        {
            return await _context.StockAlerts
                .Where(a => a.ProductId == productId && !a.IsNotified)
                .ToListAsync();
        }
    }
}
