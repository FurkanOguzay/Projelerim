using UpexTech.Entity;

namespace UpexTech.Business.Services
{
    public interface IDeviceModelService
    {
        Task<IEnumerable<DeviceModel>> GetAllAsync();
        Task<IEnumerable<DeviceModel>> GetRootModelsAsync();
        Task<IEnumerable<DeviceModel>> GetChildrenAsync(int parentId);
        Task<DeviceModel?> GetByIdAsync(int id);
        Task<DeviceModel> CreateAsync(DeviceModel model);
        Task UpdateAsync(DeviceModel model);
        Task DeleteAsync(int id);
        
        /// <summary>
        /// Tüm cihaz ağacını hiyerarşik olarak döner
        /// </summary>
        Task<IEnumerable<DeviceModel>> GetModelTreeAsync();
        
        /// <summary>
        /// Bir ürünün uyumlu olduğu modelleri döner
        /// </summary>
        Task<IEnumerable<DeviceModel>> GetProductCompatibleModelsAsync(int productId);
    }
}
