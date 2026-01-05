using Microsoft.EntityFrameworkCore;
using UpexTech.Data;
using UpexTech.Data.Repositories;
using UpexTech.Entity;

namespace UpexTech.Business.Services
{
    public class ProductService : IProductService
    {
        private readonly IRepository<Product> _productRepository;
        private readonly UpexTechDbContext _context;

        public ProductService(IRepository<Product> productRepository, UpexTechDbContext context)
        {
            _productRepository = productRepository;
            _context = context;
        }

        public async Task<IEnumerable<Product>> GetAllProductsAsync()
        {
            return await _productRepository.Query()
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Where(p => p.Stock > 0)
                .ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetAllProductsForAdminAsync()
        {
            return await _productRepository.Query()
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Include(p => p.CompatibleModels)
                    .ThenInclude(cm => cm.DeviceModel)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetPopularProductsAsync()
        {
            return await _productRepository.Query()
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Where(p => p.IsPopular && p.Stock > 0)
                .Take(4)
                .ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetImmediateDeliveryProductsAsync()
        {
            return await _productRepository.Query()
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Where(p => p.IsImmediateDelivery && p.Stock > 0)
                .Take(4)
                .ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetProductsByCategoryAsync(int categoryId)
        {
            return await _productRepository.Query()
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Where(p => p.CategoryId == categoryId && p.Stock > 0)
                .ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetProductsByBrandAsync(int brandId)
        {
            return await _productRepository.Query()
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Where(p => p.BrandId == brandId && p.Stock > 0)
                .ToListAsync();
        }

        public async Task<Product?> GetProductByIdAsync(int id)
        {
            return await _productRepository.Query()
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Include(p => p.CompatibleModels)
                    .ThenInclude(cm => cm.DeviceModel)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<Product> CreateProductAsync(Product product)
        {
            return await _productRepository.AddAsync(product);
        }

        public async Task UpdateProductAsync(Product product)
        {
            await _productRepository.UpdateAsync(product);
        }

        public async Task DeleteProductAsync(int id)
        {
            await _productRepository.DeleteAsync(id);
        }

        public async Task<IEnumerable<Product>> SearchProductsAsync(string searchTerm)
        {
            var search = searchTerm.Trim();
            return await _productRepository.Query()
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Where(p => EF.Functions.Like(p.Name, $"%{search}%") || 
                           (p.Brand != null && EF.Functions.Like(p.Brand.Name, $"%{search}%")))
                .ToListAsync();
        }

        public async Task<(IEnumerable<Product> Products, int TotalCount)> GetProductsPagedAsync(int page, int pageSize, int? categoryId = null, int? brandId = null, string? search = null)
        {
            var query = _productRepository.Query()
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Where(p => p.Stock > 0);

            if (categoryId.HasValue)
                query = query.Where(p => p.CategoryId == categoryId.Value);

            if (brandId.HasValue)
                query = query.Where(p => p.BrandId == brandId.Value);

            if (!string.IsNullOrEmpty(search))
                query = query.Where(p => p.Name.Contains(search) || (p.Description != null && p.Description.Contains(search)));

            var totalCount = await query.CountAsync();
            var products = await query
                .OrderByDescending(p => p.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (products, totalCount);
        }

        public async Task<(IEnumerable<Product> Products, bool HasMore)> GetProductsInfiniteAsync(int page, int pageSize, int? categoryId = null, int? brandId = null, string? search = null)
        {
            var query = _productRepository.Query()
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Where(p => p.Stock > 0);

            if (categoryId.HasValue)
                query = query.Where(p => p.CategoryId == categoryId.Value);

            if (brandId.HasValue)
                query = query.Where(p => p.BrandId == brandId.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchTerm = search.Trim();
                query = query.Where(p =>
                    EF.Functions.Like(p.Name, $"%{searchTerm}%") ||
                    (p.Brand != null && EF.Functions.Like(p.Brand.Name, $"%{searchTerm}%"))
                );
            }

            var totalCount = await query.CountAsync();
            var products = await query
                .OrderByDescending(p => p.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var hasMore = (page * pageSize) < totalCount;

            return (products, hasMore);
        }

        // Katalog Yönetimi Metodları
        public async Task<IEnumerable<Product>> GetCriticalStockProductsAsync()
        {
            return await _productRepository.Query()
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Where(p => p.Stock <= p.CriticalStockLevel && p.Stock > 0)
                .OrderBy(p => p.Stock)
                .ToListAsync();
        }

        public async Task<IEnumerable<Product>> SearchBySKUOrBarcodeAsync(string term)
        {
            var searchTerm = term.Trim();
            return await _productRepository.Query()
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Where(p => (p.SKU != null && EF.Functions.Like(p.SKU, $"%{searchTerm}%")) ||
                           (p.Barcode != null && EF.Functions.Like(p.Barcode, $"%{searchTerm}%")))
                .ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetByDeviceModelAsync(int deviceModelId)
        {
            return await _context.ProductDeviceModels
                .Where(pdm => pdm.DeviceModelId == deviceModelId)
                .Include(pdm => pdm.Product)
                    .ThenInclude(p => p.Category)
                .Include(pdm => pdm.Product)
                    .ThenInclude(p => p.Brand)
                .Select(pdm => pdm.Product)
                .ToListAsync();
        }

        public async Task<(IEnumerable<Product> Products, int TotalCount)> GetProductsFilteredAsync(
            int page, int pageSize,
            string? sku = null,
            string? barcode = null,
            int? deviceModelId = null,
            string? search = null,
            bool? criticalStockOnly = null)
        {
            var query = _productRepository.Query()
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .AsQueryable();

            if (!string.IsNullOrEmpty(sku))
                query = query.Where(p => p.SKU != null && EF.Functions.Like(p.SKU, $"%{sku}%"));

            if (!string.IsNullOrEmpty(barcode))
                query = query.Where(p => p.Barcode != null && EF.Functions.Like(p.Barcode, $"%{barcode}%"));

            if (!string.IsNullOrEmpty(search))
                query = query.Where(p => EF.Functions.Like(p.Name, $"%{search}%") ||
                                        (p.Brand != null && EF.Functions.Like(p.Brand.Name, $"%{search}%")));

            if (criticalStockOnly == true)
                query = query.Where(p => p.Stock <= p.CriticalStockLevel);

            if (deviceModelId.HasValue)
            {
                var productIds = await _context.ProductDeviceModels
                    .Where(pdm => pdm.DeviceModelId == deviceModelId.Value)
                    .Select(pdm => pdm.ProductId)
                    .ToListAsync();
                query = query.Where(p => productIds.Contains(p.Id));
            }

            var totalCount = await query.CountAsync();
            var products = await query
                .OrderByDescending(p => p.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (products, totalCount);
        }

        // Toplu İşlem Metodları
        public async Task BulkCreateAsync(IEnumerable<Product> products)
        {
            await _context.Products.AddRangeAsync(products);
            await _context.SaveChangesAsync();
        }

        public async Task BulkUpdateStockAsync(Dictionary<int, int> productStocks)
        {
            var productIds = productStocks.Keys.ToList();
            var products = await _context.Products
                .Where(p => productIds.Contains(p.Id))
                .ToListAsync();

            foreach (var product in products)
            {
                if (productStocks.TryGetValue(product.Id, out int newStock))
                {
                    product.Stock = newStock;
                    product.UpdatedAt = DateTime.Now;
                }
            }

            await _context.SaveChangesAsync();
        }

        public async Task UpdateCompatibleModelsAsync(int productId, List<int> deviceModelIds)
        {
            // Mevcut ilişkileri sil
            var existingRelations = await _context.ProductDeviceModels
                .Where(pdm => pdm.ProductId == productId)
                .ToListAsync();
            _context.ProductDeviceModels.RemoveRange(existingRelations);

            // Yeni ilişkileri ekle
            var newRelations = deviceModelIds.Select(dmId => new ProductDeviceModel
            {
                ProductId = productId,
                DeviceModelId = dmId
            });
            await _context.ProductDeviceModels.AddRangeAsync(newRelations);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<DeviceModel>> GetProductCompatibleModelsAsync(int productId)
        {
            return await _context.ProductDeviceModels
                .Where(pdm => pdm.ProductId == productId)
                .Include(pdm => pdm.DeviceModel)
                .Select(pdm => pdm.DeviceModel)
                .ToListAsync();
        }

        // PDP Feature Methods
        public async Task<Product?> GetProductWithDetailsAsync(int id)
        {
            return await _productRepository.Query()
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Include(p => p.Images.OrderBy(i => i.DisplayOrder))
                .Include(p => p.Variations)
                .Include(p => p.CompatibleModels)
                    .ThenInclude(cm => cm.DeviceModel)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<IEnumerable<Product>> GetRelatedProductsAsync(int productId, int count = 6)
        {
            var product = await _productRepository.Query()
                .FirstOrDefaultAsync(p => p.Id == productId);

            if (product == null)
                return Enumerable.Empty<Product>();

            // Get products from the same category (excluding current product)
            return await _productRepository.Query()
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Where(p => p.CategoryId == product.CategoryId && p.Id != productId && p.Stock > 0)
                .OrderByDescending(p => p.Rating)
                .Take(count)
                .ToListAsync();
        }

        public async Task<ProductVariation?> GetVariationAsync(int productId, VariationType type, string value)
        {
            return await _context.ProductVariations
                .FirstOrDefaultAsync(v => v.ProductId == productId && 
                                          v.VariationType == type && 
                                          v.VariationValue == value);
        }
    }
}

