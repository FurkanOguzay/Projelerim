using Microsoft.EntityFrameworkCore;
using UpexTech.Data.Repositories;
using UpexTech.Entity;

namespace UpexTech.Business.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly IRepository<Category> _categoryRepository;

        public CategoryService(IRepository<Category> categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public async Task<IEnumerable<Category>> GetAllCategoriesAsync()
        {
            return await _categoryRepository.Query()
                .OrderBy(c => c.DisplayOrder)
                .ToListAsync();
        }

        public async Task<IEnumerable<Category>> GetAllCategoriesWithBrandsAsync()
        {
            return await _categoryRepository.Query()
                .Include(c => c.Brands.Where(b => b.IsActive))
                .Where(c => c.IsActive)
                .OrderBy(c => c.DisplayOrder)
                .ToListAsync();
        }

        public async Task<Category?> GetCategoryByIdAsync(int id)
        {
            return await _categoryRepository.GetByIdAsync(id);
        }

        public async Task<Category?> GetCategoryWithBrandsAsync(int id)
        {
            return await _categoryRepository.Query()
                .Include(c => c.Brands.Where(b => b.IsActive))
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<Category> CreateCategoryAsync(Category category)
        {
            return await _categoryRepository.AddAsync(category);
        }

        public async Task UpdateCategoryAsync(Category category)
        {
            await _categoryRepository.UpdateAsync(category);
        }

        public async Task DeleteCategoryAsync(int id)
        {
            // Gerçek silme (hard delete) yap
            await _categoryRepository.HardDeleteAsync(id);
        }

        public async Task<IEnumerable<Category>> GetAllCategoriesIncludingInactiveAsync()
        {
            return await _categoryRepository.QueryAll()
                .OrderBy(c => c.DisplayOrder)
                .ToListAsync();
        }
    }
}
