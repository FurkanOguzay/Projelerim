namespace UpexTech.Entity
{
    public class FavoriteCollection : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public int UserId { get; set; }

        // Navigation Properties
        public virtual User User { get; set; } = null!;
        public virtual ICollection<FavoriteCollectionItem> Items { get; set; } = new List<FavoriteCollectionItem>();
    }

    public class FavoriteCollectionItem : BaseEntity
    {
        public int CollectionId { get; set; }
        public int FavoriteId { get; set; }

        // Navigation Properties
        public virtual FavoriteCollection Collection { get; set; } = null!;
        public virtual Favorite Favorite { get; set; } = null!;
    }
}
