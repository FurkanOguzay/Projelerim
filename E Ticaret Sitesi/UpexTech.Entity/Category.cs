namespace UpexTech.Entity
{
    public class Category : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Icon { get; set; }
        public int DisplayOrder { get; set; }

        // Navigation Properties
        public virtual ICollection<Brand> Brands { get; set; } = new List<Brand>();
        public virtual ICollection<Product> Products { get; set; } = new List<Product>();
    }
}
