namespace UpexTech.Entity
{
    public class AdminUser : BaseEntity
    {
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime? LastLoginAt { get; set; }

        // Role
        public int RoleId { get; set; }
        public virtual AdminRole Role { get; set; } = null!;

        public string FullName => $"{FirstName} {LastName}";
    }
}
