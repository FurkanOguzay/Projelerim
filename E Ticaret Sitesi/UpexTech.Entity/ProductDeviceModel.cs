namespace UpexTech.Entity
{
    /// <summary>
    /// Ürün ile Cihaz Modeli arasındaki many-to-many ilişki tablosu
    /// Bir ürünün hangi cihaz modellerine uyumlu olduğunu belirtir
    /// </summary>
    public class ProductDeviceModel
    {
        public int ProductId { get; set; }
        public int DeviceModelId { get; set; }

        // Navigation Properties
        public virtual Product Product { get; set; } = null!;
        public virtual DeviceModel DeviceModel { get; set; } = null!;
    }
}
