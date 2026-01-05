using Microsoft.EntityFrameworkCore;
using UpexTech.Data;
using UpexTech.Entity;

namespace UpexTech.Business.Services
{
    public interface IReturnService
    {
        Task<IEnumerable<Return>> GetAllReturnsAsync();
        Task<(IEnumerable<Return> Returns, int TotalCount)> GetReturnsPagedAsync(
            int page, int pageSize, ReturnStatus? status = null, string? search = null);
        Task<Return?> GetReturnByIdAsync(int id);
        Task<Return> CreateReturnAsync(Return returnRequest);
        Task UpdateReturnAsync(Return returnRequest);
        Task<bool> UpdateReturnStatusAsync(int id, ReturnStatus status, string? adminNotes = null);
        Task DeleteReturnAsync(int id);
        Task<Dictionary<ReturnStatus, int>> GetReturnCountByStatusAsync();
        Task<string> GenerateReturnNumberAsync();
    }

    public class ReturnService : IReturnService
    {
        private readonly UpexTechDbContext _context;

        public ReturnService(UpexTechDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Return>> GetAllReturnsAsync()
        {
            return await _context.Returns
                .Include(r => r.Order)
                .Include(r => r.OrderItem)
                    .ThenInclude(oi => oi.Product)
                .Include(r => r.User)
                .Where(r => r.IsActive)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<(IEnumerable<Return> Returns, int TotalCount)> GetReturnsPagedAsync(
            int page, int pageSize, ReturnStatus? status = null, string? search = null)
        {
            var query = _context.Returns
                .Include(r => r.Order)
                .Include(r => r.OrderItem)
                    .ThenInclude(oi => oi.Product)
                .Include(r => r.User)
                .Where(r => r.IsActive)
                .AsQueryable();

            if (status.HasValue)
            {
                query = query.Where(r => r.Status == status.Value);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.ToLower();
                query = query.Where(r =>
                    r.ReturnNumber.ToLower().Contains(search) ||
                    (r.User.FullName != null && r.User.FullName.ToLower().Contains(search)) ||
                    (r.OrderItem.Product.SKU != null && r.OrderItem.Product.SKU.ToLower().Contains(search)));
            }

            var totalCount = await query.CountAsync();
            var returns = await query
                .OrderByDescending(r => r.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (returns, totalCount);
        }

        public async Task<Return?> GetReturnByIdAsync(int id)
        {
            return await _context.Returns
                .Include(r => r.Order)
                .Include(r => r.OrderItem)
                    .ThenInclude(oi => oi.Product)
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Id == id && r.IsActive);
        }

        public async Task<Return> CreateReturnAsync(Return returnRequest)
        {
            returnRequest.ReturnNumber = await GenerateReturnNumberAsync();
            _context.Returns.Add(returnRequest);
            await _context.SaveChangesAsync();
            return returnRequest;
        }

        public async Task UpdateReturnAsync(Return returnRequest)
        {
            returnRequest.UpdatedAt = DateTime.Now;
            _context.Returns.Update(returnRequest);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> UpdateReturnStatusAsync(int id, ReturnStatus status, string? adminNotes = null)
        {
            var returnRequest = await _context.Returns.FindAsync(id);
            if (returnRequest == null) return false;

            returnRequest.Status = status;
            if (adminNotes != null)
            {
                returnRequest.AdminNotes = adminNotes;
            }
            returnRequest.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task DeleteReturnAsync(int id)
        {
            var returnRequest = await _context.Returns.FindAsync(id);
            if (returnRequest != null)
            {
                returnRequest.IsActive = false;
                returnRequest.UpdatedAt = DateTime.Now;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<Dictionary<ReturnStatus, int>> GetReturnCountByStatusAsync()
        {
            var counts = await _context.Returns
                .Where(r => r.IsActive)
                .GroupBy(r => r.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();

            return counts.ToDictionary(x => x.Status, x => x.Count);
        }

        public async Task<string> GenerateReturnNumberAsync()
        {
            var date = DateTime.Now.ToString("yyyyMMdd");
            var count = await _context.Returns
                .CountAsync(r => r.CreatedAt.Date == DateTime.Today);
            return $"IAD-{date}-{(count + 1):D4}";
        }
    }
}
