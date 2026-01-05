namespace UpexTech.Entity
{
    public class ProductImage : BaseEntity
    {
        public int ProductId { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public int DisplayOrder { get; set; }
        public bool IsMain { get; set; }
        public string? AltText { get; set; }

        // Navigation Property
        public virtual Product Product { get; set; } = null!;
    }
}
