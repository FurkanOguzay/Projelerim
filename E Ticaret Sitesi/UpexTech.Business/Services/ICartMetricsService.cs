using UpexTech.Business.DTOs;

namespace UpexTech.Business.Services
{
    public interface ICartMetricsService
    {
        /// <summary>
        /// Müşteri davranışı raporunu getirir (sepet ve favori metrikleri)
        /// </summary>
        /// <returns>Ürün bazında rapor verileri, Net Satış Adedi'ne göre sıralı</returns>
        Task<IEnumerable<CartBehaviorReportDto>> GetCartBehaviorReportAsync();
    }
}
