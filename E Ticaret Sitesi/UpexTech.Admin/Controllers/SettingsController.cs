using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UpexTech.Business.Services;
using UpexTech.Entity;

namespace UpexTech.Admin.Controllers
{
    [Authorize]
    public class SettingsController : AdminBaseController
    {
        private readonly IAdminUserService _adminUserService;

        public SettingsController(IAdminUserService adminUserService)
        {
            _adminUserService = adminUserService;
        }

        // Genel Ayarlar sayfası
        public IActionResult Index()
        {
            return View();
        }

        // Yöneticiler & Roller sayfası
        public async Task<IActionResult> Roles()
        {
            var adminUsers = await _adminUserService.GetAllAsync();
            var roles = await _adminUserService.GetAllRolesAsync();
            
            ViewBag.Roles = roles;
            return View(adminUsers);
        }

        #region Admin User API

        [HttpPost]
        public async Task<IActionResult> CreateAdmin([FromBody] CreateAdminRequest request)
        {
            try
            {
                // Email kontrolü
                var existing = await _adminUserService.GetByEmailAsync(request.Email);
                if (existing != null)
                {
                    return Json(new { success = false, message = "Bu e-posta adresi zaten kullanılıyor." });
                }

                var user = new AdminUser
                {
                    Email = request.Email,
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    Phone = request.Phone,
                    RoleId = request.RoleId,
                    IsActive = true
                };

                await _adminUserService.CreateAsync(user, request.Password);
                return Json(new { success = true, message = "Yönetici başarıyla oluşturuldu." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateAdmin([FromBody] UpdateAdminRequest request)
        {
            try
            {
                var user = await _adminUserService.GetByIdAsync(request.Id);
                if (user == null)
                {
                    return Json(new { success = false, message = "Kullanıcı bulunamadı." });
                }

                user.FirstName = request.FirstName;
                user.LastName = request.LastName;
                user.Email = request.Email;
                user.Phone = request.Phone;
                user.RoleId = request.RoleId;
                user.IsActive = request.IsActive;

                await _adminUserService.UpdateAsync(user);
                return Json(new { success = true, message = "Yönetici başarıyla güncellendi." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteAdmin(int id)
        {
            try
            {
                await _adminUserService.DeleteAsync(id);
                return Json(new { success = true, message = "Yönetici başarıyla silindi." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAdmin(int id)
        {
            var user = await _adminUserService.GetByIdAsync(id);
            if (user == null)
            {
                return Json(new { success = false, message = "Kullanıcı bulunamadı." });
            }

            return Json(new 
            { 
                success = true, 
                data = new 
                {
                    user.Id,
                    user.Email,
                    user.FirstName,
                    user.LastName,
                    user.Phone,
                    user.RoleId,
                    user.IsActive
                }
            });
        }

        #endregion

        #region Role API

        [HttpPost]
        public async Task<IActionResult> CreateRole([FromBody] CreateRoleRequest request)
        {
            try
            {
                var permissions = request.Permissions.Aggregate(AdminPermission.None, (current, p) => current | p);

                var role = new AdminRole
                {
                    Name = request.Name,
                    Description = request.Description,
                    Permissions = permissions,
                    IsSystemRole = false
                };

                await _adminUserService.CreateRoleAsync(role);
                return Json(new { success = true, message = "Rol başarıyla oluşturuldu." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateRole([FromBody] UpdateRoleRequest request)
        {
            try
            {
                var role = await _adminUserService.GetRoleByIdAsync(request.Id);
                if (role == null)
                {
                    return Json(new { success = false, message = "Rol bulunamadı." });
                }

                var permissions = request.Permissions.Aggregate(AdminPermission.None, (current, p) => current | p);

                role.Name = request.Name;
                role.Description = request.Description;
                role.Permissions = permissions;

                await _adminUserService.UpdateRoleAsync(role);
                return Json(new { success = true, message = "Rol başarıyla güncellendi." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteRole(int id)
        {
            try
            {
                await _adminUserService.DeleteRoleAsync(id);
                return Json(new { success = true, message = "Rol başarıyla silindi." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetRole(int id)
        {
            var role = await _adminUserService.GetRoleByIdAsync(id);
            if (role == null)
            {
                return Json(new { success = false, message = "Rol bulunamadı." });
            }

            // Permission flag'lerini ayrıştır
            var permissions = new List<int>();
            foreach (AdminPermission p in Enum.GetValues(typeof(AdminPermission)))
            {
                if (p != AdminPermission.None && p != AdminPermission.All && (role.Permissions & p) == p)
                {
                    permissions.Add((int)p);
                }
            }

            return Json(new 
            { 
                success = true, 
                data = new 
                {
                    role.Id,
                    role.Name,
                    role.Description,
                    role.IsSystemRole,
                    Permissions = permissions
                }
            });
        }

        #endregion
    }

    // Request Models
    public class CreateAdminRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public int RoleId { get; set; }
    }

    public class UpdateAdminRequest
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public int RoleId { get; set; }
        public bool IsActive { get; set; }
    }

    public class CreateRoleRequest
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public List<AdminPermission> Permissions { get; set; } = new();
    }

    public class UpdateRoleRequest
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public List<AdminPermission> Permissions { get; set; } = new();
    }
}
