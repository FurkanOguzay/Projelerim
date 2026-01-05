using UpexTech.Business.DTOs;

namespace UpexTech.Business.Services
{
    public interface IPerformanceReportService
    {
        /// <summary>
        /// Genel performans özeti (KPI kartları için)
        /// </summary>
        Task<GeneralPerformanceSummaryDto> GetGeneralPerformanceSummaryAsync(string segment, DateTime startDate, DateTime endDate);

        /// <summary>
        /// Satış grafiği verileri (Line Chart için)
        /// </summary>
        Task<List<SalesChartDataDto>> GetSalesChartDataAsync(string segment, DateTime startDate, DateTime endDate, string breakdown);

        /// <summary>
        /// İl bazında satış dağılımı
        /// </summary>
        Task<List<CitySalesDto>> GetCitySalesDistributionAsync(string segment, DateTime startDate, DateTime endDate);

        /// <summary>
        /// Platform dağılımı (Mobil Web vs Desktop)
        /// </summary>
        Task<PlatformDistributionDto> GetPlatformDistributionAsync(string segment, DateTime startDate, DateTime endDate);

        /// <summary>
        /// Ürün performans listesi
        /// </summary>
        Task<List<ProductPerformanceDto>> GetProductPerformanceAsync(ProductPerformanceFilterDto filter);
    }
}
