using UpexTech.Entity;

namespace UpexTech.Business.Services
{
    public interface IUserService
    {
        Task<User?> AuthenticateAsync(string email, string password);
        Task<User?> GetUserByIdAsync(int id);
        Task<User?> GetByIdAsync(int id);
        Task<User?> GetUserByEmailAsync(string email);
        Task<IEnumerable<User>> GetAllUsersAsync();
        Task<IEnumerable<User>> GetPendingDealersAsync();
        Task<User> RegisterAsync(User user, string password);
        Task UpdateUserAsync(User user);
        Task UpdateAsync(User user);
        Task ApproveDealerAsync(int userId);
        Task RejectDealerAsync(int userId);
        Task<bool> EmailExistsAsync(string email);
        Task<(bool Success, string ErrorMessage)> ChangePasswordAsync(int userId, string currentPassword, string newPassword);

        // B2B Yönetimi için yeni metodlar
        Task<IEnumerable<User>> GetAllCustomersAsync();  // Admin hariç tüm kullanıcılar
        Task<IEnumerable<User>> GetB2BCustomersAsync();  // Sadece B2B bayiler
        Task<IEnumerable<User>> GetB2CCustomersAsync();  // Sadece B2C müşteriler
        Task<User?> GetCustomerWithOrdersAsync(int id);  // Siparişlerle birlikte
        Task UpdateCustomerTierAsync(int userId, CustomerTier tier);
        Task UpdateCreditLimitAsync(int userId, decimal limit);
        Task UpdatePriceListAsync(int userId, int? priceListId, string? priceListName);
    }
}

