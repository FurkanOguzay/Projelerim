using UpexTech.Entity;

namespace UpexTech.Business.Services
{
    public interface IProductService
    {
        Task<IEnumerable<Product>> GetAllProductsAsync();
        Task<IEnumerable<Product>> GetAllProductsForAdminAsync(); // Stok filtresi olmadan
        Task<IEnumerable<Product>> GetPopularProductsAsync();
        Task<IEnumerable<Product>> GetImmediateDeliveryProductsAsync();
        Task<IEnumerable<Product>> GetProductsByCategoryAsync(int categoryId);
        Task<IEnumerable<Product>> GetProductsByBrandAsync(int brandId);
        Task<Product?> GetProductByIdAsync(int id);
        Task<Product> CreateProductAsync(Product product);
        Task UpdateProductAsync(Product product);
        Task DeleteProductAsync(int id);
        Task<IEnumerable<Product>> SearchProductsAsync(string searchTerm);
        
        // Sayfalama için yeni metodlar
        Task<(IEnumerable<Product> Products, int TotalCount)> GetProductsPagedAsync(int page, int pageSize, int? categoryId = null, int? brandId = null, string? search = null);
        Task<(IEnumerable<Product> Products, bool HasMore)> GetProductsInfiniteAsync(int page, int pageSize, int? categoryId = null, int? brandId = null, string? search = null);

        // Katalog Yönetimi için yeni metodlar
        Task<IEnumerable<Product>> GetCriticalStockProductsAsync();
        Task<IEnumerable<Product>> SearchBySKUOrBarcodeAsync(string term);
        Task<IEnumerable<Product>> GetByDeviceModelAsync(int deviceModelId);
        Task<(IEnumerable<Product> Products, int TotalCount)> GetProductsFilteredAsync(
            int page, int pageSize, 
            string? sku = null, 
            string? barcode = null, 
            int? deviceModelId = null,
            string? search = null,
            bool? criticalStockOnly = null);
        
        // Toplu işlemler
        Task BulkCreateAsync(IEnumerable<Product> products);
        Task BulkUpdateStockAsync(Dictionary<int, int> productStocks);
        Task UpdateCompatibleModelsAsync(int productId, List<int> deviceModelIds);
        Task<IEnumerable<DeviceModel>> GetProductCompatibleModelsAsync(int productId);

        // PDP Feature Methods
        Task<Product?> GetProductWithDetailsAsync(int id);  // With Images, Variations
        Task<IEnumerable<Product>> GetRelatedProductsAsync(int productId, int count = 6);  // Cross-sell
        Task<ProductVariation?> GetVariationAsync(int productId, VariationType type, string value);
    }
}

