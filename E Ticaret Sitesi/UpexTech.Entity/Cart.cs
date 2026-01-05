namespace UpexTech.Entity
{
    public class Cart : BaseEntity
    {
        // Foreign Keys
        public int UserId { get; set; }

        // Navigation Properties
        public virtual User User { get; set; } = null!;
        public virtual ICollection<CartItem> Items { get; set; } = new List<CartItem>();

        // Computed Properties
        public decimal TotalAmount => Items.Where(i => i.IsActive).Sum(i => i.TotalPrice);
        public int TotalItems => Items.Where(i => i.IsActive).Sum(i => i.Quantity);
    }

    public class CartItem : BaseEntity
    {
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice => Quantity * UnitPrice;

        // Foreign Keys
        public int CartId { get; set; }
        public int ProductId { get; set; }

        // Navigation Properties
        public virtual Cart Cart { get; set; } = null!;
        public virtual Product Product { get; set; } = null!;
    }
}
