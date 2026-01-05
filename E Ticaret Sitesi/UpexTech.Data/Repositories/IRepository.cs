using System.Linq.Expressions;
using UpexTech.Entity;

namespace UpexTech.Data.Repositories
{
    public interface IRepository<T> where T : BaseEntity
    {
        Task<T?> GetByIdAsync(int id);
        Task<IEnumerable<T>> GetAllAsync();
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
        Task<T> AddAsync(T entity);
        Task UpdateAsync(T entity);
        Task DeleteAsync(int id);
        Task HardDeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        IQueryable<T> Query();
        IQueryable<T> QueryAll();
    }
}
