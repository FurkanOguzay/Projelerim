namespace UpexTech.Entity
{
    public class Favorite : BaseEntity
    {
        // Foreign Keys
        public int UserId { get; set; }
        public int ProductId { get; set; }

        // Navigation Properties
        public virtual User User { get; set; } = null!;
        public virtual Product Product { get; set; } = null!;
        public virtual ICollection<FavoriteCollectionItem> CollectionItems { get; set; } = new List<FavoriteCollectionItem>();
    }
}
