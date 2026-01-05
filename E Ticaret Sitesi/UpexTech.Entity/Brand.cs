namespace UpexTech.Entity
{
    public class Brand : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string? Logo { get; set; }
        public string? Description { get; set; }

        // Foreign Keys
        public int CategoryId { get; set; }

        // Navigation Properties
        public virtual Category? Category { get; set; }
        public virtual ICollection<Product> Products { get; set; } = new List<Product>();
    }
}
