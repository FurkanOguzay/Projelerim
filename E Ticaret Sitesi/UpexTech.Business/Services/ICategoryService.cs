using UpexTech.Entity;

namespace UpexTech.Business.Services
{
    public interface ICategoryService
    {
        Task<IEnumerable<Category>> GetAllCategoriesAsync();
        Task<IEnumerable<Category>> GetAllCategoriesWithBrandsAsync();
        Task<IEnumerable<Category>> GetAllCategoriesIncludingInactiveAsync();
        Task<Category?> GetCategoryByIdAsync(int id);
        Task<Category?> GetCategoryWithBrandsAsync(int id);
        Task<Category> CreateCategoryAsync(Category category);
        Task UpdateCategoryAsync(Category category);
        Task DeleteCategoryAsync(int id);
    }
}
