using Microsoft.EntityFrameworkCore;
using UpexTech.Business.Services;
using UpexTech.Data;
using UpexTech.Data.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Set App_Data path for database file - use shared App_Data at solution root
var appDataPath = Path.GetFullPath(Path.Combine(builder.Environment.ContentRootPath, "..", "App_Data"));
if (!Directory.Exists(appDataPath))
    Directory.CreateDirectory(appDataPath);

// Use SQLite database file in App_Data folder
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
Console.WriteLine($"[ADMIN] Connecting to SQLite database");

// Add services to the container.
builder.Services.AddControllersWithViews();

// Database Context
builder.Services.AddDbContext<UpexTechDbContext>(options =>
    options.UseSqlite(connectionString)
           .ConfigureWarnings(warnings => warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning)));

// Repositories
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

// Services
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IBrandService, BrandService>();
builder.Services.AddScoped<IDeviceModelService, DeviceModelService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IFavoriteService, FavoriteService>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<ICartMetricsService, CartMetricsService>();
builder.Services.AddScoped<IExcelService, ExcelService>();
builder.Services.AddScoped<IAccountTransactionService, AccountTransactionService>();
builder.Services.AddScoped<IBannerService, BannerService>();
builder.Services.AddScoped<ISalesReportService, SalesReportService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IQuoteService, QuoteService>();
builder.Services.AddScoped<IReturnService, ReturnService>();
builder.Services.AddScoped<IPriceListService, PriceListService>();
builder.Services.AddScoped<IAdminUserService, AdminUserService>();
builder.Services.AddScoped<IPerformanceReportService, PerformanceReportService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();

// Session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Authentication
builder.Services.AddAuthentication("AdminCookies")
    .AddCookie("AdminCookies", options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(24);
        options.Cookie.Name = "UpexTech.Admin.Auth";
    });

var app = builder.Build();

// Ensure Payments table exists (for SQLite)
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<UpexTechDbContext>();
    try
    {
        // Create Payments table if it doesn't exist
        context.Database.ExecuteSqlRaw(@"
            CREATE TABLE IF NOT EXISTS Payments (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                UserId INTEGER NOT NULL,
                OrderId INTEGER,
                Amount REAL NOT NULL,
                PaymentMethod INTEGER NOT NULL,
                Status INTEGER NOT NULL DEFAULT 1,
                Channel INTEGER NOT NULL,
                PaymentDate TEXT NOT NULL,
                ReferenceNumber TEXT,
                Description TEXT,
                BankName TEXT,
                AccountName TEXT,
                InstallmentCount INTEGER,
                IsIncoming INTEGER NOT NULL DEFAULT 1,
                IsActive INTEGER NOT NULL DEFAULT 1,
                CreatedAt TEXT NOT NULL DEFAULT (datetime('now')),
                UpdatedAt TEXT,
                FOREIGN KEY (UserId) REFERENCES Users(Id),
                FOREIGN KEY (OrderId) REFERENCES Orders(Id)
            );
        ");
        Console.WriteLine("[ADMIN] Payments table verified/created.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[ADMIN] Note: Could not verify Payments table: {ex.Message}");
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Dashboard}/{action=Index}/{id?}");

app.Run();
