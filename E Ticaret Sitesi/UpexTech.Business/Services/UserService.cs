using Microsoft.EntityFrameworkCore;
using UpexTech.Data.Repositories;
using UpexTech.Entity;

namespace UpexTech.Business.Services
{
    public class UserService : IUserService
    {
        private readonly IRepository<User> _userRepository;

        public UserService(IRepository<User> userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<User?> AuthenticateAsync(string email, string password)
        {
            var user = await _userRepository.Query()
                .FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
                return null;

            // Gerçek projede password hash kontrolü yapılacak
            if (user.PasswordHash != password)
                return null;

            return user;
        }

        public async Task<User?> GetUserByIdAsync(int id)
        {
            return await _userRepository.GetByIdAsync(id);
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await _userRepository.Query()
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            return await _userRepository.Query()
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<User>> GetPendingDealersAsync()
        {
            return await _userRepository.Query()
                .Where(u => u.Role == UserRole.B2B && u.Status == UserStatus.Pending)
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync();
        }

        public async Task<User> RegisterAsync(User user, string password)
        {
            // Gerçek projede password hash'lenecek
            user.PasswordHash = password;
            
            // Bayi ise onay bekliyor durumunda
            if (user.Role == UserRole.B2B)
            {
                user.Status = UserStatus.Pending;
            }
            else
            {
                user.Status = UserStatus.Active;
            }

            return await _userRepository.AddAsync(user);
        }

        public async Task UpdateUserAsync(User user)
        {
            await _userRepository.UpdateAsync(user);
        }

        public async Task ApproveDealerAsync(int userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user != null && user.Role == UserRole.B2B)
            {
                user.Status = UserStatus.Active;
                await _userRepository.UpdateAsync(user);
            }
        }

        public async Task RejectDealerAsync(int userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user != null && user.Role == UserRole.B2B)
            {
                user.Status = UserStatus.Rejected;
                await _userRepository.UpdateAsync(user);
            }
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _userRepository.Query()
                .AnyAsync(u => u.Email == email);
        }

        public async Task<User?> GetByIdAsync(int id)
        {
            return await _userRepository.GetByIdAsync(id);
        }

        public async Task UpdateAsync(User user)
        {
            await _userRepository.UpdateAsync(user);
        }

        public async Task<(bool Success, string ErrorMessage)> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                return (false, "Kullanıcı bulunamadı.");

            // Mevcut şifre kontrolü (gerçek projede hash kontrolü yapılacak)
            if (user.PasswordHash != currentPassword)
                return (false, "Mevcut şifre hatalı.");

            // Yeni şifre güvenlik validasyonu
            var passwordValidation = Helpers.PasswordValidator.Validate(newPassword);
            if (!passwordValidation.IsValid)
                return (false, passwordValidation.ErrorMessage);

            // Yeni şifreyi kaydet (gerçek projede hash'lenecek)
            user.PasswordHash = newPassword;
            await _userRepository.UpdateAsync(user);
            
            return (true, string.Empty);
        }

        // B2B Yönetimi için yeni metodlar
        public async Task<IEnumerable<User>> GetAllCustomersAsync()
        {
            return await _userRepository.Query()
                .Where(u => u.Role != UserRole.Admin)
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<User>> GetB2BCustomersAsync()
        {
            return await _userRepository.Query()
                .Where(u => u.Role == UserRole.B2B && u.Status == UserStatus.Active)
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<User>> GetB2CCustomersAsync()
        {
            return await _userRepository.Query()
                .Where(u => u.Role == UserRole.B2C)
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync();
        }

        public async Task<User?> GetCustomerWithOrdersAsync(int id)
        {
            return await _userRepository.Query()
                .Include(u => u.Orders)
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task UpdateCustomerTierAsync(int userId, CustomerTier tier)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user != null)
            {
                user.Tier = tier;
                user.UpdatedAt = DateTime.Now;
                await _userRepository.UpdateAsync(user);
            }
        }

        public async Task UpdateCreditLimitAsync(int userId, decimal limit)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user != null)
            {
                user.CreditLimit = limit;
                user.UpdatedAt = DateTime.Now;
                await _userRepository.UpdateAsync(user);
            }
        }

        public async Task UpdatePriceListAsync(int userId, int? priceListId, string? priceListName)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user != null)
            {
                user.PriceListId = priceListId;
                user.PriceListName = priceListName;
                user.UpdatedAt = DateTime.Now;
                await _userRepository.UpdateAsync(user);
            }
        }
    }
}

