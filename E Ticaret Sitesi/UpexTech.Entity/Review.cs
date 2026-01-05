namespace UpexTech.Entity
{
    public class Review : BaseEntity
    {
        public int Rating { get; set; }  // 1-5 yıldız
        public string? Title { get; set; }
        public string Comment { get; set; } = string.Empty;

        // Foreign Keys
        public int ProductId { get; set; }
        public int UserId { get; set; }

        // Navigation Properties
        public virtual Product Product { get; set; } = null!;
        public virtual User User { get; set; } = null!;
    }
}
