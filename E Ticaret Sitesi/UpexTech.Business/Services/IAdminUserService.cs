using UpexTech.Entity;

namespace UpexTech.Business.Services
{
    public interface IAdminUserService
    {
        // Admin User operations
        Task<AdminUser?> AuthenticateAsync(string email, string password);
        Task<List<AdminUser>> GetAllAsync();
        Task<AdminUser?> GetByIdAsync(int id);
        Task<AdminUser?> GetByEmailAsync(string email);
        Task<AdminUser> CreateAsync(AdminUser user, string password);
        Task<AdminUser> UpdateAsync(AdminUser user);
        Task<bool> UpdatePasswordAsync(int userId, string newPassword);
        Task<bool> DeleteAsync(int id);
        
        // Role operations
        Task<List<AdminRole>> GetAllRolesAsync();
        Task<AdminRole?> GetRoleByIdAsync(int id);
        Task<AdminRole> CreateRoleAsync(AdminRole role);
        Task<AdminRole> UpdateRoleAsync(AdminRole role);
        Task<bool> DeleteRoleAsync(int id);
        
        // Permission check
        bool HasPermission(AdminUser user, AdminPermission permission);
    }
}
