using Microsoft.EntityFrameworkCore;
using UpexTech.Data;
using UpexTech.Entity;

namespace UpexTech.Business.Services
{
    public class AdminUserService : IAdminUserService
    {
        private readonly UpexTechDbContext _context;

        public AdminUserService(UpexTechDbContext context)
        {
            _context = context;
        }

        #region Admin User Operations

        public async Task<AdminUser?> AuthenticateAsync(string email, string password)
        {
            var user = await _context.AdminUsers
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Email == email && u.IsActive);

            if (user == null || user.PasswordHash != password)
                return null;

            // Update last login
            user.LastLoginAt = DateTime.Now;
            await _context.SaveChangesAsync();

            return user;
        }

        public async Task<List<AdminUser>> GetAllAsync()
        {
            return await _context.AdminUsers
                .Include(u => u.Role)
                .OrderBy(u => u.FirstName)
                .ToListAsync();
        }

        public async Task<AdminUser?> GetByIdAsync(int id)
        {
            return await _context.AdminUsers
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<AdminUser?> GetByEmailAsync(string email)
        {
            return await _context.AdminUsers
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<AdminUser> CreateAsync(AdminUser user, string password)
        {
            user.PasswordHash = password; // Gerçek projede hash'lenecek
            user.CreatedAt = DateTime.Now;
            user.IsActive = true;

            _context.AdminUsers.Add(user);
            await _context.SaveChangesAsync();

            return user;
        }

        public async Task<AdminUser> UpdateAsync(AdminUser user)
        {
            var existing = await _context.AdminUsers.FindAsync(user.Id);
            if (existing == null)
                throw new Exception("Kullanıcı bulunamadı");

            existing.FirstName = user.FirstName;
            existing.LastName = user.LastName;
            existing.Email = user.Email;
            existing.Phone = user.Phone;
            existing.RoleId = user.RoleId;
            existing.IsActive = user.IsActive;
            existing.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> UpdatePasswordAsync(int userId, string newPassword)
        {
            var user = await _context.AdminUsers.FindAsync(userId);
            if (user == null) return false;

            user.PasswordHash = newPassword; // Gerçek projede hash'lenecek
            user.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var user = await _context.AdminUsers.Include(u => u.Role).FirstOrDefaultAsync(u => u.Id == id);
            if (user == null) return false;

            // Süper Admin silinemez
            if (user.Role.IsSystemRole && user.Email == "admin@upextech.com")
                throw new Exception("Sistem yöneticisi silinemez!");

            _context.AdminUsers.Remove(user);
            await _context.SaveChangesAsync();
            return true;
        }

        #endregion

        #region Role Operations

        public async Task<List<AdminRole>> GetAllRolesAsync()
        {
            return await _context.AdminRoles
                .Include(r => r.AdminUsers)
                .OrderBy(r => r.Id)
                .ToListAsync();
        }

        public async Task<AdminRole?> GetRoleByIdAsync(int id)
        {
            return await _context.AdminRoles
                .Include(r => r.AdminUsers)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<AdminRole> CreateRoleAsync(AdminRole role)
        {
            role.CreatedAt = DateTime.Now;
            _context.AdminRoles.Add(role);
            await _context.SaveChangesAsync();
            return role;
        }

        public async Task<AdminRole> UpdateRoleAsync(AdminRole role)
        {
            var existing = await _context.AdminRoles.FindAsync(role.Id);
            if (existing == null)
                throw new Exception("Rol bulunamadı");

            // Sistem rollerinin adı değiştirilemez
            if (!existing.IsSystemRole)
            {
                existing.Name = role.Name;
            }
            
            existing.Description = role.Description;
            existing.Permissions = role.Permissions;
            existing.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> DeleteRoleAsync(int id)
        {
            var role = await _context.AdminRoles.Include(r => r.AdminUsers).FirstOrDefaultAsync(r => r.Id == id);
            if (role == null) return false;

            // Sistem rolü silinemez
            if (role.IsSystemRole)
                throw new Exception("Sistem rolleri silinemez!");

            // Kullanıcısı olan rol silinemez
            if (role.AdminUsers.Any())
                throw new Exception("Bu role atanmış kullanıcılar var. Önce kullanıcıları başka bir role taşıyın.");

            _context.AdminRoles.Remove(role);
            await _context.SaveChangesAsync();
            return true;
        }

        #endregion

        #region Permission Check

        public bool HasPermission(AdminUser user, AdminPermission permission)
        {
            if (user?.Role == null) return false;
            
            // All permission her şeye izin verir
            if (user.Role.Permissions == AdminPermission.All) return true;
            
            return (user.Role.Permissions & permission) == permission;
        }

        #endregion
    }
}
