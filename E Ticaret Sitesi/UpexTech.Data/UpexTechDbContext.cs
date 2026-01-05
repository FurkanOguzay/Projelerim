using Microsoft.EntityFrameworkCore;
using UpexTech.Entity;

namespace UpexTech.Data
{
    public class UpexTechDbContext : DbContext
    {
        public UpexTechDbContext(DbContextOptions<UpexTechDbContext> options) : base(options)
        {
        }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Brand> Brands { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Favorite> Favorites { get; set; }
        public DbSet<FavoriteCollection> FavoriteCollections { get; set; }
        public DbSet<FavoriteCollectionItem> FavoriteCollectionItems { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<AccountTransaction> AccountTransactions { get; set; }
        public DbSet<DeviceModel> DeviceModels { get; set; }
        public DbSet<ProductDeviceModel> ProductDeviceModels { get; set; }
        
        // PDP Feature DbSets
        public DbSet<ProductImage> ProductImages { get; set; }
        public DbSet<ProductVariation> ProductVariations { get; set; }
        public DbSet<StockAlert> StockAlerts { get; set; }
        
        // Banner Management
        public DbSet<Banner> Banners { get; set; }
        
        // Quote Management (B2B Teklif)
        public DbSet<Quote> Quotes { get; set; }
        public DbSet<QuoteItem> QuoteItems { get; set; }
        
        // Return Management (İade)
        public DbSet<Return> Returns { get; set; }
        
        // Price List Management
        public DbSet<PriceList> PriceLists { get; set; }
        public DbSet<CustomerGroup> CustomerGroups { get; set; }
        public DbSet<CustomerGroupPriceList> CustomerGroupPriceLists { get; set; }
        
        // Review System
        public DbSet<Review> Reviews { get; set; }
        
        // Browsing History
        public DbSet<BrowsingHistory> BrowsingHistories { get; set; }
        
        // Saved Cards
        public DbSet<SavedCard> SavedCards { get; set; }
        
        // Admin Panel - Yönetici ve Roller
        public DbSet<AdminRole> AdminRoles { get; set; }
        public DbSet<AdminUser> AdminUsers { get; set; }
        
        // Payment Management
        public DbSet<Payment> Payments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Category
            modelBuilder.Entity<Category>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.Icon).HasMaxLength(50);
            });

            // Brand
            modelBuilder.Entity<Brand>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Logo).HasMaxLength(255);
                entity.HasOne(e => e.Category)
                    .WithMany(c => c.Brands)
                    .HasForeignKey(e => e.CategoryId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Product
            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Description).HasMaxLength(2000);
                entity.Property(e => e.Image).HasMaxLength(255);
                entity.Property(e => e.SKU).HasMaxLength(50);
                entity.Property(e => e.Barcode).HasMaxLength(50);
                entity.Property(e => e.PriceB2C).HasColumnType("decimal(18,2)");
                entity.Property(e => e.PriceB2B).HasColumnType("decimal(18,2)");
                entity.Property(e => e.PurchasePrice).HasColumnType("decimal(18,2)");
                entity.HasOne(e => e.Category)
                    .WithMany(c => c.Products)
                    .HasForeignKey(e => e.CategoryId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.Brand)
                    .WithMany(b => b.Products)
                    .HasForeignKey(e => e.BrandId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // DeviceModel (Cihaz Modeli Hiyerarşisi)
            modelBuilder.Entity<DeviceModel>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.HasOne(e => e.Parent)
                    .WithMany(e => e.Children)
                    .HasForeignKey(e => e.ParentId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ProductDeviceModel (Many-to-Many)
            modelBuilder.Entity<ProductDeviceModel>(entity =>
            {
                entity.HasKey(e => new { e.ProductId, e.DeviceModelId });
                entity.HasOne(e => e.Product)
                    .WithMany(p => p.CompatibleModels)
                    .HasForeignKey(e => e.ProductId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.DeviceModel)
                    .WithMany(d => d.Products)
                    .HasForeignKey(e => e.DeviceModelId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // User
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.PasswordHash).IsRequired().HasMaxLength(255);
                entity.Property(e => e.FirstName).IsRequired().HasMaxLength(50);
                entity.Property(e => e.LastName).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Phone).HasMaxLength(20);
                entity.Property(e => e.CompanyName).HasMaxLength(200);
                entity.Property(e => e.TaxNumber).HasMaxLength(20);
                entity.Property(e => e.Address).HasMaxLength(500);
            });

            // Favorite
            modelBuilder.Entity<Favorite>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.UserId, e.ProductId }).IsUnique();
                entity.HasOne(e => e.User)
                    .WithMany(u => u.Favorites)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.Product)
                    .WithMany(p => p.Favorites)
                    .HasForeignKey(e => e.ProductId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // FavoriteCollection
            modelBuilder.Entity<FavoriteCollection>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // FavoriteCollectionItem
            modelBuilder.Entity<FavoriteCollectionItem>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.CollectionId, e.FavoriteId }).IsUnique();
                entity.HasOne(e => e.Collection)
                    .WithMany(c => c.Items)
                    .HasForeignKey(e => e.CollectionId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.Favorite)
                    .WithMany(f => f.CollectionItems)
                    .HasForeignKey(e => e.FavoriteId)
                    .OnDelete(DeleteBehavior.NoAction); // Cascade yerine NoAction
            });

            // Order
            modelBuilder.Entity<Order>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.OrderNumber).IsRequired().HasMaxLength(50);
                entity.HasIndex(e => e.OrderNumber).IsUnique();
                entity.Property(e => e.TotalAmount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.ShippingAddress).HasMaxLength(500);
                entity.HasOne(e => e.User)
                    .WithMany(u => u.Orders)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // OrderItem
            modelBuilder.Entity<OrderItem>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.UnitPrice).HasColumnType("decimal(18,2)");
                entity.HasOne(e => e.Order)
                    .WithMany(o => o.OrderItems)
                    .HasForeignKey(e => e.OrderId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.Product)
                    .WithMany(p => p.OrderItems)
                    .HasForeignKey(e => e.ProductId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Cart
            modelBuilder.Entity<Cart>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.UserId).IsUnique();
                entity.HasOne(e => e.User)
                    .WithOne()
                    .HasForeignKey<Cart>(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // CartItem
            modelBuilder.Entity<CartItem>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.UnitPrice).HasColumnType("decimal(18,2)");
                entity.HasIndex(e => new { e.CartId, e.ProductId }).IsUnique();
                entity.HasOne(e => e.Cart)
                    .WithMany(c => c.Items)
                    .HasForeignKey(e => e.CartId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.Product)
                    .WithMany()
                    .HasForeignKey(e => e.ProductId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // AccountTransaction
            modelBuilder.Entity<AccountTransaction>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Amount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.ReferenceNumber).HasMaxLength(50);
                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.Order)
                    .WithMany()
                    .HasForeignKey(e => e.OrderId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ProductImage
            modelBuilder.Entity<ProductImage>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ImageUrl).IsRequired().HasMaxLength(500);
                entity.Property(e => e.AltText).HasMaxLength(200);
                entity.HasOne(e => e.Product)
                    .WithMany(p => p.Images)
                    .HasForeignKey(e => e.ProductId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ProductVariation
            modelBuilder.Entity<ProductVariation>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.VariationValue).IsRequired().HasMaxLength(100);
                entity.Property(e => e.ColorCode).HasMaxLength(10);
                entity.Property(e => e.SKU).HasMaxLength(50);
                entity.Property(e => e.ImageUrl).HasMaxLength(500);
                entity.Property(e => e.PriceAdjustment).HasColumnType("decimal(18,2)");
                entity.HasOne(e => e.Product)
                    .WithMany(p => p.Variations)
                    .HasForeignKey(e => e.ProductId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // StockAlert
            modelBuilder.Entity<StockAlert>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
                entity.HasOne(e => e.Product)
                    .WithMany(p => p.StockAlerts)
                    .HasForeignKey(e => e.ProductId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.ProductVariation)
                    .WithMany()
                    .HasForeignKey(e => e.ProductVariationId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Banner
            modelBuilder.Entity<Banner>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
                entity.Property(e => e.ImageUrl).IsRequired().HasMaxLength(500);
                entity.Property(e => e.TargetUrl).HasMaxLength(500);
                entity.Property(e => e.Position).IsRequired();
                entity.Property(e => e.StartDate).IsRequired();
                entity.Property(e => e.EndDate).IsRequired();
            });

            // Quote
            modelBuilder.Entity<Quote>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.QuoteNumber).IsRequired().HasMaxLength(50);
                entity.HasIndex(e => e.QuoteNumber).IsUnique();
                entity.Property(e => e.TotalAmount).HasColumnType("decimal(18,2)");
                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // QuoteItem
            modelBuilder.Entity<QuoteItem>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.UnitPrice).HasColumnType("decimal(18,2)");
                entity.Property(e => e.DiscountPercentage).HasColumnType("decimal(5,2)");
                entity.HasOne(e => e.Quote)
                    .WithMany(q => q.Items)
                    .HasForeignKey(e => e.QuoteId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.Product)
                    .WithMany()
                    .HasForeignKey(e => e.ProductId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Return
            modelBuilder.Entity<Return>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ReturnNumber).IsRequired().HasMaxLength(50);
                entity.HasIndex(e => e.ReturnNumber).IsUnique();
                entity.Property(e => e.RefundAmount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.ReasonDescription).HasMaxLength(500);
                entity.Property(e => e.TrackingNumber).HasMaxLength(100);
                entity.Property(e => e.AttachmentPath).HasMaxLength(500);
                entity.Property(e => e.AdminNotes).HasMaxLength(1000);
                entity.HasOne(e => e.Order)
                    .WithMany()
                    .HasForeignKey(e => e.OrderId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.OrderItem)
                    .WithMany()
                    .HasForeignKey(e => e.OrderItemId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // PriceList
            modelBuilder.Entity<PriceList>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.Factor).HasColumnType("decimal(5,2)");
                entity.HasOne(e => e.BasePriceList)
                    .WithMany()
                    .HasForeignKey(e => e.BasePriceListId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // CustomerGroup
            modelBuilder.Entity<CustomerGroup>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.DiscountPercentage).HasColumnType("decimal(5,2)");
            });

            // CustomerGroupPriceList
            modelBuilder.Entity<CustomerGroupPriceList>(entity =>
            {
                entity.HasKey(e => new { e.CustomerGroupId, e.PriceListId });
                entity.HasOne(e => e.CustomerGroup)
                    .WithMany(cg => cg.PriceLists)
                    .HasForeignKey(e => e.CustomerGroupId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.PriceList)
                    .WithMany(pl => pl.CustomerGroups)
                    .HasForeignKey(e => e.PriceListId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Review
            modelBuilder.Entity<Review>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Rating).IsRequired();
                entity.Property(e => e.Title).HasMaxLength(200);
                entity.Property(e => e.Comment).IsRequired().HasMaxLength(2000);
                entity.HasIndex(e => new { e.UserId, e.ProductId }).IsUnique(); // Her kullanıcı ürün başına 1 yorum
                entity.HasOne(e => e.Product)
                    .WithMany(p => p.Reviews)
                    .HasForeignKey(e => e.ProductId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.User)
                    .WithMany(u => u.Reviews)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // BrowsingHistory
            modelBuilder.Entity<BrowsingHistory>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ViewedAt).IsRequired();
                entity.HasOne(e => e.Product)
                    .WithMany()
                    .HasForeignKey(e => e.ProductId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // SavedCard
            modelBuilder.Entity<SavedCard>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.CardNumber).IsRequired().HasMaxLength(20);
                entity.Property(e => e.CardHolderName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.ExpiryMonth).IsRequired().HasMaxLength(2);
                entity.Property(e => e.ExpiryYear).IsRequired().HasMaxLength(2);
                entity.Property(e => e.CardType).IsRequired().HasMaxLength(20);
                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // AdminRole
            modelBuilder.Entity<AdminRole>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500);
            });

            // AdminUser
            modelBuilder.Entity<AdminUser>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.PasswordHash).IsRequired().HasMaxLength(255);
                entity.Property(e => e.FirstName).IsRequired().HasMaxLength(50);
                entity.Property(e => e.LastName).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Phone).HasMaxLength(20);
                entity.HasOne(e => e.Role)
                    .WithMany(r => r.AdminUsers)
                    .HasForeignKey(e => e.RoleId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Payment
            modelBuilder.Entity<Payment>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Amount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.ReferenceNumber).HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.BankName).HasMaxLength(100);
                entity.Property(e => e.AccountName).HasMaxLength(100);
                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.Order)
                    .WithMany()
                    .HasForeignKey(e => e.OrderId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Seed Data
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // Admin User (eski sistem için korunuyor)
            modelBuilder.Entity<User>().HasData(new User
            {
                Id = 1,
                Email = "admin@upextech.com",
                PasswordHash = "admin123", // Gerçek projede hash'lenecek
                FirstName = "Admin",
                LastName = "User",
                Role = UserRole.Admin,
                Status = UserStatus.Active,
                CreatedAt = DateTime.Now
            });

            // Admin Roles (Yeni RBAC sistemi)
            modelBuilder.Entity<AdminRole>().HasData(
                new AdminRole 
                { 
                    Id = 1, 
                    Name = "Süper Admin", 
                    Description = "Tüm sistem yetkilerine sahip en üst düzey yönetici",
                    IsSystemRole = true,
                    Permissions = AdminPermission.All,
                    CreatedAt = DateTime.Now 
                },
                new AdminRole 
                { 
                    Id = 2, 
                    Name = "Muhasebeci", 
                    Description = "Finans, muhasebe ve cari hesap işlemleri",
                    IsSystemRole = false,
                    Permissions = AdminPermission.Dashboard | AdminPermission.Finance | AdminPermission.CariHesap | AdminPermission.Reports,
                    CreatedAt = DateTime.Now 
                },
                new AdminRole 
                { 
                    Id = 3, 
                    Name = "Stokçu", 
                    Description = "Ürün ve envanter yönetimi",
                    IsSystemRole = false,
                    Permissions = AdminPermission.Dashboard | AdminPermission.Products | AdminPermission.Categories | AdminPermission.DeviceModels | AdminPermission.BulkOperations,
                    CreatedAt = DateTime.Now 
                },
                new AdminRole 
                { 
                    Id = 4, 
                    Name = "Müşteri Temsilcisi", 
                    Description = "Müşteri, sipariş ve iade işlemleri",
                    IsSystemRole = false,
                    Permissions = AdminPermission.Dashboard | AdminPermission.Orders | AdminPermission.Returns | AdminPermission.Customers | AdminPermission.Members | AdminPermission.Dealers,
                    CreatedAt = DateTime.Now 
                },
                new AdminRole 
                { 
                    Id = 5, 
                    Name = "Pazarlamacı", 
                    Description = "Banner ve kampanya yönetimi",
                    IsSystemRole = false,
                    Permissions = AdminPermission.Dashboard | AdminPermission.Banners | AdminPermission.Reports | AdminPermission.PriceList,
                    CreatedAt = DateTime.Now 
                }
            );

            // Admin Users (Yeni RBAC sistemi)
            modelBuilder.Entity<AdminUser>().HasData(
                new AdminUser 
                { 
                    Id = 1, 
                    Email = "admin@upextech.com",
                    PasswordHash = "admin123", 
                    FirstName = "Süper",
                    LastName = "Admin",
                    RoleId = 1, // Süper Admin
                    IsActive = true,
                    CreatedAt = DateTime.Now 
                }
            );


            // Categories
            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Akıllı Telefonlar", Icon = "bi-phone", DisplayOrder = 1, CreatedAt = DateTime.Now },
                new Category { Id = 2, Name = "Tabletler", Icon = "bi-tablet", DisplayOrder = 2, CreatedAt = DateTime.Now },
                new Category { Id = 3, Name = "Aksesuarlar", Icon = "bi-headphones", DisplayOrder = 3, CreatedAt = DateTime.Now }
            );

            // Brands
            modelBuilder.Entity<Brand>().HasData(
                new Brand { Id = 1, Name = "Apple", CategoryId = 1, CreatedAt = DateTime.Now },
                new Brand { Id = 2, Name = "Samsung", CategoryId = 1, CreatedAt = DateTime.Now },
                new Brand { Id = 3, Name = "Xiaomi", CategoryId = 1, CreatedAt = DateTime.Now },
                new Brand { Id = 4, Name = "Huawei", CategoryId = 1, CreatedAt = DateTime.Now }
            );

            // Products
            modelBuilder.Entity<Product>().HasData(
                new Product { Id = 1, Name = "Apple iPhone 17 256GB", Description = "A19 Pro çip, ProMotion ekran, Titanyum tasarım", Image = "iphone-17.webp", Stock = 50, PriceB2C = 89999, PriceB2B = 81000, PurchasePrice = 72000, SKU = "APL-IP17-256", Rating = 4.9, ReviewCount = 325, CategoryId = 1, BrandId = 1, IsPopular = true, CreatedAt = DateTime.Now },
                new Product { Id = 2, Name = "Apple iPhone Air 128GB", Description = "Ultra ince tasarım, M3 çip, 48MP kamera", Image = "iphone-air.webp", Stock = 40, PriceB2C = 74999, PriceB2B = 67500, PurchasePrice = 60000, SKU = "APL-IPAIR-128", Rating = 4.8, ReviewCount = 280, CategoryId = 1, BrandId = 1, IsPopular = true, CreatedAt = DateTime.Now },
                new Product { Id = 3, Name = "Samsung Galaxy S25 Ultra 512GB", Description = "Snapdragon 8 Gen 4, 200MP kamera, S Pen dahil", Image = "galaxy-s25-ultra.png", Stock = 60, PriceB2C = 84999, PriceB2B = 76500, PurchasePrice = 68000, SKU = "SMS-GS25U-512", Rating = 4.9, ReviewCount = 410, CategoryId = 1, BrandId = 2, IsPopular = true, CreatedAt = DateTime.Now },
                new Product { Id = 4, Name = "Samsung Galaxy S25 256GB", Description = "Galaxy AI, 50MP kamera, Dynamic AMOLED 2X", Image = "galaxy-s25.png", Stock = 55, PriceB2C = 54999, PriceB2B = 49500, PurchasePrice = 44000, SKU = "SMS-GS25-256", Rating = 4.7, ReviewCount = 290, CategoryId = 1, BrandId = 2, IsPopular = true, CreatedAt = DateTime.Now },
                new Product { Id = 5, Name = "Xiaomi 15 Pro 512GB", Description = "Leica kamera, Snapdragon 8 Elite, 120W hızlı şarj", Image = "Xiaomi-15-pro.webp", Stock = 45, PriceB2C = 44999, PriceB2B = 40500, PurchasePrice = 36000, SKU = "XMI-15P-512", Rating = 4.8, ReviewCount = 195, CategoryId = 1, BrandId = 3, IsImmediateDelivery = true, CreatedAt = DateTime.Now },
                new Product { Id = 6, Name = "Xiaomi 15 256GB", Description = "Leica optik, LTPO AMOLED, 5000mAh batarya", Image = "Xiaomi-15.webp", Stock = 35, PriceB2C = 34999, PriceB2B = 31500, PurchasePrice = 28000, SKU = "XMI-15-256", Rating = 4.7, ReviewCount = 180, CategoryId = 1, BrandId = 3, IsImmediateDelivery = true, CreatedAt = DateTime.Now }
            );

            // DeviceModels (Cihaz Hiyerarşisi)
            // Level 0: Markalar, Level 1: Seriler, Level 2: Modeller
            modelBuilder.Entity<DeviceModel>().HasData(
                // Apple Markası
                new DeviceModel { Id = 1, Name = "Apple", Level = 0, ParentId = null, DisplayOrder = 1, CreatedAt = DateTime.Now },
                // Apple iPhone Serileri
                new DeviceModel { Id = 2, Name = "iPhone 15 Serisi", Level = 1, ParentId = 1, DisplayOrder = 1, CreatedAt = DateTime.Now },
                new DeviceModel { Id = 3, Name = "iPhone 14 Serisi", Level = 1, ParentId = 1, DisplayOrder = 2, CreatedAt = DateTime.Now },
                new DeviceModel { Id = 4, Name = "iPhone 13 Serisi", Level = 1, ParentId = 1, DisplayOrder = 3, CreatedAt = DateTime.Now },
                new DeviceModel { Id = 5, Name = "iPhone 12 Serisi", Level = 1, ParentId = 1, DisplayOrder = 4, CreatedAt = DateTime.Now },
                new DeviceModel { Id = 6, Name = "iPhone 11 Serisi", Level = 1, ParentId = 1, DisplayOrder = 5, CreatedAt = DateTime.Now },
                // iPhone 15 Modelleri
                new DeviceModel { Id = 7, Name = "iPhone 15", Level = 2, ParentId = 2, DisplayOrder = 1, CreatedAt = DateTime.Now },
                new DeviceModel { Id = 8, Name = "iPhone 15 Plus", Level = 2, ParentId = 2, DisplayOrder = 2, CreatedAt = DateTime.Now },
                new DeviceModel { Id = 9, Name = "iPhone 15 Pro", Level = 2, ParentId = 2, DisplayOrder = 3, CreatedAt = DateTime.Now },
                new DeviceModel { Id = 10, Name = "iPhone 15 Pro Max", Level = 2, ParentId = 2, DisplayOrder = 4, CreatedAt = DateTime.Now },
                // iPhone 14 Modelleri
                new DeviceModel { Id = 11, Name = "iPhone 14", Level = 2, ParentId = 3, DisplayOrder = 1, CreatedAt = DateTime.Now },
                new DeviceModel { Id = 12, Name = "iPhone 14 Plus", Level = 2, ParentId = 3, DisplayOrder = 2, CreatedAt = DateTime.Now },
                new DeviceModel { Id = 13, Name = "iPhone 14 Pro", Level = 2, ParentId = 3, DisplayOrder = 3, CreatedAt = DateTime.Now },
                new DeviceModel { Id = 14, Name = "iPhone 14 Pro Max", Level = 2, ParentId = 3, DisplayOrder = 4, CreatedAt = DateTime.Now },
                // iPhone 13 Modelleri
                new DeviceModel { Id = 15, Name = "iPhone 13", Level = 2, ParentId = 4, DisplayOrder = 1, CreatedAt = DateTime.Now },
                new DeviceModel { Id = 16, Name = "iPhone 13 Mini", Level = 2, ParentId = 4, DisplayOrder = 2, CreatedAt = DateTime.Now },
                new DeviceModel { Id = 17, Name = "iPhone 13 Pro", Level = 2, ParentId = 4, DisplayOrder = 3, CreatedAt = DateTime.Now },
                new DeviceModel { Id = 18, Name = "iPhone 13 Pro Max", Level = 2, ParentId = 4, DisplayOrder = 4, CreatedAt = DateTime.Now },
                
                // Samsung Markası
                new DeviceModel { Id = 19, Name = "Samsung", Level = 0, ParentId = null, DisplayOrder = 2, CreatedAt = DateTime.Now },
                // Samsung Galaxy Serileri
                new DeviceModel { Id = 20, Name = "Galaxy S24 Serisi", Level = 1, ParentId = 19, DisplayOrder = 1, CreatedAt = DateTime.Now },
                new DeviceModel { Id = 21, Name = "Galaxy S23 Serisi", Level = 1, ParentId = 19, DisplayOrder = 2, CreatedAt = DateTime.Now },
                // Galaxy S24 Modelleri
                new DeviceModel { Id = 22, Name = "Galaxy S24", Level = 2, ParentId = 20, DisplayOrder = 1, CreatedAt = DateTime.Now },
                new DeviceModel { Id = 23, Name = "Galaxy S24+", Level = 2, ParentId = 20, DisplayOrder = 2, CreatedAt = DateTime.Now },
                new DeviceModel { Id = 24, Name = "Galaxy S24 Ultra", Level = 2, ParentId = 20, DisplayOrder = 3, CreatedAt = DateTime.Now },
                // Galaxy S23 Modelleri
                new DeviceModel { Id = 25, Name = "Galaxy S23", Level = 2, ParentId = 21, DisplayOrder = 1, CreatedAt = DateTime.Now },
                new DeviceModel { Id = 26, Name = "Galaxy S23+", Level = 2, ParentId = 21, DisplayOrder = 2, CreatedAt = DateTime.Now },
                new DeviceModel { Id = 27, Name = "Galaxy S23 Ultra", Level = 2, ParentId = 21, DisplayOrder = 3, CreatedAt = DateTime.Now }
            );

            // PriceList - Standart Liste (1 çarpanlı varsayılan liste)
            modelBuilder.Entity<PriceList>().HasData(
                new PriceList 
                { 
                    Id = 1, 
                    Name = "Standart Liste", 
                    Description = "Varsayılan fiyat listesi - 1x çarpan", 
                    DisplayOrder = 1, 
                    Factor = 1.00m, 
                    Rounding = RoundingMethod.None,
                    BasePriceListId = null,
                    IsActive = true,
                    CreatedAt = DateTime.Now 
                }
            );
        }
    }
}
