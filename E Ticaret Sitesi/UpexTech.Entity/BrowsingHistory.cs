using System;

namespace UpexTech.Entity
{
    public class BrowsingHistory : BaseEntity
    {
        public int UserId { get; set; }
        public int ProductId { get; set; }
        public DateTime ViewedAt { get; set; }
        
        // Navigation Properties
        public virtual User User { get; set; } = null!;
        public virtual Product Product { get; set; } = null!;
    }
}
