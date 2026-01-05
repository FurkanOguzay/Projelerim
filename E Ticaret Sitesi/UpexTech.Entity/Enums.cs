namespace UpexTech.Entity
{
    public enum UserRole
    {
        Admin = 1,
        B2C = 2,    // Perakende müşteri
        B2B = 3     // Bayi
    }

    public enum UserStatus
    {
        Pending = 1,    // Onay bekliyor (Bayi için)
        Active = 2,
        Inactive = 3,
        Rejected = 4
    }

    public enum CustomerTier
    {
        Standard = 1,   // Standart müşteri
        Silver = 2,     // Gümüş bayi
        Gold = 3,       // Altın bayi
        Platinum = 4    // Platin bayi
    }

    [Flags]
    public enum AdminPermission
    {
        None = 0,
        Dashboard = 1,
        Orders = 2,
        Returns = 4,
        Products = 8,
        Categories = 16,
        DeviceModels = 32,
        BulkOperations = 64,
        CariHesap = 128,
        Customers = 256,
        Members = 512,
        Dealers = 1024,
        PriceList = 2048,
        Reports = 4096,
        Finance = 8192,
        Banners = 16384,
        Settings = 32768,
        AdminUsers = 65536,  // Sadece SuperAdmin - diğer adminleri yönetebilir
        All = int.MaxValue   // SuperAdmin için tüm izinler
    }
}

