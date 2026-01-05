using Microsoft.EntityFrameworkCore;
using UpexTech.Data;
using UpexTech.Entity;

namespace UpexTech.Business.Services
{
    public interface IReviewService
    {
        Task<IEnumerable<Review>> GetReviewsByProductIdAsync(int productId);
        Task<IEnumerable<Review>> GetUserReviewsAsync(int userId);
        Task<Review?> GetReviewByIdAsync(int reviewId);
        Task<Review> AddReviewAsync(Review review);
        Task<bool> UpdateReviewAsync(Review review);
        Task<bool> DeleteReviewAsync(int reviewId);
        Task<bool> HasUserReviewedProductAsync(int userId, int productId);
        Task<(double AverageRating, int TotalCount, Dictionary<int, int> Distribution)> GetProductRatingStatsAsync(int productId);
    }

    public class ReviewService : IReviewService
    {
        private readonly UpexTechDbContext _context;

        public ReviewService(UpexTechDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Review>> GetReviewsByProductIdAsync(int productId)
        {
            return await _context.Reviews
                .Include(r => r.User)
                .Where(r => r.ProductId == productId && r.IsActive)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Review>> GetUserReviewsAsync(int userId)
        {
            return await _context.Reviews
                .Include(r => r.Product)
                .Where(r => r.UserId == userId && r.IsActive)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<Review?> GetReviewByIdAsync(int reviewId)
        {
            return await _context.Reviews
                .Include(r => r.User)
                .Include(r => r.Product)
                .FirstOrDefaultAsync(r => r.Id == reviewId && r.IsActive);
        }

        public async Task<Review> AddReviewAsync(Review review)
        {
            review.CreatedAt = DateTime.Now;
            review.IsActive = true;
            
            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();

            // Ürünün ortalama puanını ve yorum sayısını güncelle
            await UpdateProductRatingAsync(review.ProductId);

            return review;
        }

        public async Task<bool> UpdateReviewAsync(Review review)
        {
            var existingReview = await _context.Reviews.FindAsync(review.Id);
            if (existingReview == null) return false;

            existingReview.Rating = review.Rating;
            existingReview.Title = review.Title;
            existingReview.Comment = review.Comment;
            existingReview.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            // Ürünün ortalama puanını güncelle
            await UpdateProductRatingAsync(existingReview.ProductId);

            return true;
        }

        public async Task<bool> DeleteReviewAsync(int reviewId)
        {
            var review = await _context.Reviews.FindAsync(reviewId);
            if (review == null) return false;

            review.IsActive = false;
            review.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();

            // Ürünün ortalama puanını güncelle
            await UpdateProductRatingAsync(review.ProductId);

            return true;
        }

        public async Task<bool> HasUserReviewedProductAsync(int userId, int productId)
        {
            return await _context.Reviews
                .AnyAsync(r => r.UserId == userId && r.ProductId == productId && r.IsActive);
        }

        public async Task<(double AverageRating, int TotalCount, Dictionary<int, int> Distribution)> GetProductRatingStatsAsync(int productId)
        {
            var reviews = await _context.Reviews
                .Where(r => r.ProductId == productId && r.IsActive)
                .ToListAsync();

            if (!reviews.Any())
            {
                return (0, 0, new Dictionary<int, int> { { 5, 0 }, { 4, 0 }, { 3, 0 }, { 2, 0 }, { 1, 0 } });
            }

            var average = reviews.Average(r => r.Rating);
            var count = reviews.Count;
            var distribution = reviews
                .GroupBy(r => r.Rating)
                .ToDictionary(g => g.Key, g => g.Count());

            // Eksik rating değerlerini 0 ile doldur
            for (int i = 1; i <= 5; i++)
            {
                if (!distribution.ContainsKey(i))
                    distribution[i] = 0;
            }

            return (Math.Round(average, 1), count, distribution);
        }

        private async Task UpdateProductRatingAsync(int productId)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null) return;

            var reviews = await _context.Reviews
                .Where(r => r.ProductId == productId && r.IsActive)
                .ToListAsync();

            if (reviews.Any())
            {
                product.Rating = Math.Round(reviews.Average(r => r.Rating), 1);
                product.ReviewCount = reviews.Count;
            }
            else
            {
                product.Rating = 0;
                product.ReviewCount = 0;
            }

            product.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();
        }
    }
}
