using Microsoft.EntityFrameworkCore;
using UpexTech.Business.Services;
using UpexTech.Data;
using UpexTech.Data.Repositories;
using UpexTech.Entity;

var builder = WebApplication.CreateBuilder(args);

// Set App_Data path for database file - use shared App_Data at solution root
var appDataPath = Path.GetFullPath(Path.Combine(builder.Environment.ContentRootPath, "..", "App_Data"));
if (!Directory.Exists(appDataPath))
    Directory.CreateDirectory(appDataPath);

// Use SQLite database file in App_Data folder
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
Console.WriteLine($"[WEB] Connecting to SQLite database");

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
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IFavoriteService, FavoriteService>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IAccountTransactionService, AccountTransactionService>();
builder.Services.AddScoped<IStockAlertService, StockAlertService>();
builder.Services.AddScoped<IReviewService, ReviewService>();
builder.Services.AddScoped<IBrowsingHistoryService, BrowsingHistoryService>();
builder.Services.AddScoped<ISavedCardService, SavedCardService>();
builder.Services.AddScoped<IPriceListService, PriceListService>();

// Session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Authentication
builder.Services.AddAuthentication("Cookies")
    .AddCookie("Cookies", options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(24);
    });

var app = builder.Build();

// Auto create database - ÖNCE veritabanını oluştur
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<UpexTechDbContext>();
    try
    {
        db.Database.EnsureCreated(); // Yeni DB oluştur
        Console.WriteLine("Veritabanı başarıyla oluşturuldu/kontrol edildi!");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Veritabanı zaten mevcut veya bağlantı kuruldu: {ex.Message}");
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
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
