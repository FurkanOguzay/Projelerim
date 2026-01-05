using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using UpexTech.Data.Repositories;
using UpexTech.Entity;

namespace UpexTech.Business.Services
{
    public interface IBannerService
    {
        Task<IEnumerable<Banner>> GetAllBannersAsync();
        Task<IEnumerable<Banner>> GetActiveBannersAsync(BannerPosition? position = null);
        Task<Banner?> GetBannerByIdAsync(int id);
        Task<Banner> CreateBannerAsync(Banner banner);
        Task UpdateBannerAsync(Banner banner);
        Task DeleteBannerAsync(int id);
        Task<string> UploadImageAsync(IFormFile file, string webRootPath);
    }

    public class BannerService : IBannerService
    {
        private readonly IRepository<Banner> _bannerRepository;

        public BannerService(IRepository<Banner> bannerRepository)
        {
            _bannerRepository = bannerRepository;
        }

        public async Task<IEnumerable<Banner>> GetAllBannersAsync()
        {
            return await _bannerRepository.Query()
                .OrderBy(b => b.DisplayOrder)
                .ThenByDescending(b => b.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Banner>> GetActiveBannersAsync(BannerPosition? position = null)
        {
            var now = DateTime.Now;
            var query = _bannerRepository.Query()
                .Where(b => b.IsActive && b.StartDate <= now && b.EndDate >= now);

            if (position.HasValue)
            {
                query = query.Where(b => b.Position == position.Value);
            }

            return await query
                .OrderBy(b => b.DisplayOrder)
                .ToListAsync();
        }

        public async Task<Banner?> GetBannerByIdAsync(int id)
        {
            return await _bannerRepository.Query()
                .FirstOrDefaultAsync(b => b.Id == id);
        }

        public async Task<Banner> CreateBannerAsync(Banner banner)
        {
            return await _bannerRepository.AddAsync(banner);
        }

        public async Task UpdateBannerAsync(Banner banner)
        {
            await _bannerRepository.UpdateAsync(banner);
        }

        public async Task DeleteBannerAsync(int id)
        {
            await _bannerRepository.DeleteAsync(id);
        }

        public async Task<string> UploadImageAsync(IFormFile file, string webRootPath)
        {
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("Dosya seçilmedi.");
            }

            // Validate file type
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension))
            {
                throw new ArgumentException("Geçersiz dosya türü. Sadece JPG, PNG, GIF ve WebP dosyaları yüklenebilir.");
            }

            // Validate file size (max 5MB)
            if (file.Length > 5 * 1024 * 1024)
            {
                throw new ArgumentException("Dosya boyutu 5MB'dan büyük olamaz.");
            }

            // Create uploads directory if not exists
            var uploadsFolder = Path.Combine(webRootPath, "uploads", "banners");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            // Generate unique filename
            var uniqueFileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            // Save file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Return relative URL
            return $"/uploads/banners/{uniqueFileName}";
        }
    }
}
