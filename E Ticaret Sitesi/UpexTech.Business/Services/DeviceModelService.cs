using Microsoft.EntityFrameworkCore;
using UpexTech.Data;
using UpexTech.Entity;

namespace UpexTech.Business.Services
{
    public class DeviceModelService : IDeviceModelService
    {
        private readonly UpexTechDbContext _context;

        public DeviceModelService(UpexTechDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<DeviceModel>> GetAllAsync()
        {
            return await _context.DeviceModels
                .Include(d => d.Parent)
                .Include(d => d.Children)
                .OrderBy(d => d.DisplayOrder)
                .ToListAsync();
        }

        public async Task<IEnumerable<DeviceModel>> GetRootModelsAsync()
        {
            return await _context.DeviceModels
                .Where(d => d.ParentId == null)
                .OrderBy(d => d.DisplayOrder)
                .ToListAsync();
        }

        public async Task<IEnumerable<DeviceModel>> GetChildrenAsync(int parentId)
        {
            return await _context.DeviceModels
                .Where(d => d.ParentId == parentId)
                .OrderBy(d => d.DisplayOrder)
                .ToListAsync();
        }

        public async Task<DeviceModel?> GetByIdAsync(int id)
        {
            return await _context.DeviceModels
                .Include(d => d.Parent)
                .Include(d => d.Children)
                .FirstOrDefaultAsync(d => d.Id == id);
        }

        public async Task<DeviceModel> CreateAsync(DeviceModel model)
        {
            model.CreatedAt = DateTime.Now;
            _context.DeviceModels.Add(model);
            await _context.SaveChangesAsync();
            return model;
        }

        public async Task UpdateAsync(DeviceModel model)
        {
            model.UpdatedAt = DateTime.Now;
            _context.DeviceModels.Update(model);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var model = await _context.DeviceModels.FindAsync(id);
            if (model != null)
            {
                _context.DeviceModels.Remove(model);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<DeviceModel>> GetModelTreeAsync()
        {
            // Kök modelleri al ve alt modellerini recursive olarak yükle
            return await _context.DeviceModels
                .Include(d => d.Children)
                    .ThenInclude(c => c.Children)
                        .ThenInclude(c => c.Children)
                .Where(d => d.ParentId == null)
                .OrderBy(d => d.DisplayOrder)
                .ToListAsync();
        }

        public async Task<IEnumerable<DeviceModel>> GetProductCompatibleModelsAsync(int productId)
        {
            return await _context.ProductDeviceModels
                .Where(pdm => pdm.ProductId == productId)
                .Select(pdm => pdm.DeviceModel)
                .ToListAsync();
        }
    }
}
