using Microsoft.EntityFrameworkCore;
using UpexTech.Data.Repositories;
using UpexTech.Entity;

namespace UpexTech.Business.Services
{
    public interface IBrandService
    {
        Task<IEnumerable<Brand>> GetAllBrandsAsync();
        Task<IEnumerable<Brand>> GetBrandsByCategoryAsync(int categoryId);
        Task<Brand?> GetBrandByIdAsync(int id);
        Task<Brand> CreateBrandAsync(Brand brand);
        Task UpdateBrandAsync(Brand brand);
        Task DeleteBrandAsync(int id);
    }

    public class BrandService : IBrandService
    {
        private readonly IRepository<Brand> _brandRepository;

        public BrandService(IRepository<Brand> brandRepository)
        {
            _brandRepository = brandRepository;
        }

        public async Task<IEnumerable<Brand>> GetAllBrandsAsync()
        {
            return await _brandRepository.Query()
                .Include(b => b.Category)
                .ToListAsync();
        }

        public async Task<IEnumerable<Brand>> GetBrandsByCategoryAsync(int categoryId)
        {
            return await _brandRepository.Query()
                .Where(b => b.CategoryId == categoryId)
                .ToListAsync();
        }

        public async Task<Brand?> GetBrandByIdAsync(int id)
        {
            return await _brandRepository.Query()
                .Include(b => b.Category)
                .FirstOrDefaultAsync(b => b.Id == id);
        }

        public async Task<Brand> CreateBrandAsync(Brand brand)
        {
            return await _brandRepository.AddAsync(brand);
        }

        public async Task UpdateBrandAsync(Brand brand)
        {
            await _brandRepository.UpdateAsync(brand);
        }

        public async Task DeleteBrandAsync(int id)
        {
            await _brandRepository.DeleteAsync(id);
        }
    }
}
