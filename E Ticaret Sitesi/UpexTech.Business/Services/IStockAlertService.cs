using UpexTech.Entity;

namespace UpexTech.Business.Services
{
    public interface IStockAlertService
    {
        Task<StockAlert> CreateAlertAsync(int productId, int? productVariationId, int? userId, string email);
        Task<bool> HasAlertAsync(int productId, int? productVariationId, string email);
        Task<IEnumerable<StockAlert>> GetAlertsForProductAsync(int productId);
    }
}
