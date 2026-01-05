namespace UpexTech.Entity
{
    /// <summary>
    /// Cihaz modeli hiyerarşisi (Marka > Seri > Model)
    /// Örnek: Apple > iPhone 13 > Pro Max
    /// </summary>
    public class DeviceModel : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        
        /// <summary>
        /// Üst model ID (null = kök eleman, örn: Apple, Samsung)
        /// </summary>
        public int? ParentId { get; set; }
        
        /// <summary>
        /// Hiyerarşi seviyesi: 0=Marka, 1=Seri, 2=Model
        /// </summary>
        public int Level { get; set; }
        
        public int DisplayOrder { get; set; }

        // Navigation Properties
        public virtual DeviceModel? Parent { get; set; }
        public virtual ICollection<DeviceModel> Children { get; set; } = new List<DeviceModel>();
        public virtual ICollection<ProductDeviceModel> Products { get; set; } = new List<ProductDeviceModel>();
    }
}
