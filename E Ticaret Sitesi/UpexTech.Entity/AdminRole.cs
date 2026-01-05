namespace UpexTech.Entity
{
    public class AdminRole : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsSystemRole { get; set; } = false; // SuperAdmin gibi silinemeyen roller
        public AdminPermission Permissions { get; set; } = AdminPermission.Dashboard;

        // Navigation
        public virtual ICollection<AdminUser> AdminUsers { get; set; } = new List<AdminUser>();
    }
}
