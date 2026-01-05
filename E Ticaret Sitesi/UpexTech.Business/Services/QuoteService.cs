using Microsoft.EntityFrameworkCore;
using UpexTech.Data;
using UpexTech.Entity;

namespace UpexTech.Business.Services
{
    public interface IQuoteService
    {
        Task<IEnumerable<Quote>> GetAllQuotesAsync();
        Task<(IEnumerable<Quote> Quotes, int TotalCount)> GetQuotesPagedAsync(
            int page, int pageSize, QuoteStatus? status = null, string? search = null);
        Task<Quote?> GetQuoteByIdAsync(int id);
        Task<Quote> CreateQuoteAsync(Quote quote);
        Task UpdateQuoteAsync(Quote quote);
        Task<bool> UpdateQuoteStatusAsync(int id, QuoteStatus status);
        Task DeleteQuoteAsync(int id);
        Task<Dictionary<QuoteStatus, int>> GetQuoteCountByStatusAsync();
        Task<string> GenerateQuoteNumberAsync();
    }

    public class QuoteService : IQuoteService
    {
        private readonly UpexTechDbContext _context;

        public QuoteService(UpexTechDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Quote>> GetAllQuotesAsync()
        {
            return await _context.Quotes
                .Include(q => q.Items)
                    .ThenInclude(i => i.Product)
                .Include(q => q.User)
                .Where(q => q.IsActive)
                .OrderByDescending(q => q.CreatedAt)
                .ToListAsync();
        }

        public async Task<(IEnumerable<Quote> Quotes, int TotalCount)> GetQuotesPagedAsync(
            int page, int pageSize, QuoteStatus? status = null, string? search = null)
        {
            var query = _context.Quotes
                .Include(q => q.Items)
                    .ThenInclude(i => i.Product)
                .Include(q => q.User)
                .Where(q => q.IsActive)
                .AsQueryable();

            if (status.HasValue)
            {
                query = query.Where(q => q.Status == status.Value);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.ToLower();
                query = query.Where(q =>
                    q.QuoteNumber.ToLower().Contains(search) ||
                    (q.User.CompanyName != null && q.User.CompanyName.ToLower().Contains(search)) ||
                    (q.User.FullName != null && q.User.FullName.ToLower().Contains(search)));
            }

            var totalCount = await query.CountAsync();
            var quotes = await query
                .OrderByDescending(q => q.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (quotes, totalCount);
        }

        public async Task<Quote?> GetQuoteByIdAsync(int id)
        {
            return await _context.Quotes
                .Include(q => q.Items)
                    .ThenInclude(i => i.Product)
                .Include(q => q.User)
                .FirstOrDefaultAsync(q => q.Id == id && q.IsActive);
        }

        public async Task<Quote> CreateQuoteAsync(Quote quote)
        {
            quote.QuoteNumber = await GenerateQuoteNumberAsync();
            _context.Quotes.Add(quote);
            await _context.SaveChangesAsync();
            return quote;
        }

        public async Task UpdateQuoteAsync(Quote quote)
        {
            quote.UpdatedAt = DateTime.Now;
            _context.Quotes.Update(quote);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> UpdateQuoteStatusAsync(int id, QuoteStatus status)
        {
            var quote = await _context.Quotes.FindAsync(id);
            if (quote == null) return false;

            quote.Status = status;
            quote.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task DeleteQuoteAsync(int id)
        {
            var quote = await _context.Quotes.FindAsync(id);
            if (quote != null)
            {
                quote.IsActive = false;
                quote.UpdatedAt = DateTime.Now;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<Dictionary<QuoteStatus, int>> GetQuoteCountByStatusAsync()
        {
            var counts = await _context.Quotes
                .Where(q => q.IsActive)
                .GroupBy(q => q.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();

            return counts.ToDictionary(x => x.Status, x => x.Count);
        }

        public async Task<string> GenerateQuoteNumberAsync()
        {
            var date = DateTime.Now.ToString("yyyyMMdd");
            var count = await _context.Quotes
                .CountAsync(q => q.CreatedAt.Date == DateTime.Today);
            return $"TEK-{date}-{(count + 1):D4}";
        }
    }
}
