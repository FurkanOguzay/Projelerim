using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UpexTech.Data;
using UpexTech.Entity;

namespace UpexTech.Admin.Controllers
{
    [Authorize]
    public class MemberController : AdminBaseController
    {
        private readonly UpexTechDbContext _context;

        public MemberController(UpexTechDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string? memberId, string? name, string? email, string? city, string? status, int page = 1)
        {
            var query = _context.Users
                .Where(u => u.Role == UserRole.B2C) // B2C customers only
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(memberId))
            {
                // M00001 formatından ID'yi çıkar
                var idSearch = memberId.Replace("M", "").TrimStart('0');
                if (int.TryParse(idSearch, out int idNum))
                {
                    query = query.Where(u => u.Id == idNum);
                }
            }

            if (!string.IsNullOrEmpty(name))
            {
                query = query.Where(u => 
                    u.FirstName.Contains(name) || 
                    u.LastName.Contains(name) ||
                    (u.FirstName + " " + u.LastName).Contains(name));
            }

            if (!string.IsNullOrEmpty(email))
            {
                query = query.Where(u => u.Email.Contains(email));
            }

            if (!string.IsNullOrEmpty(city))
            {
                query = query.Where(u => u.Address != null && u.Address.Contains(city));
            }

            if (!string.IsNullOrEmpty(status))
            {
                if (status.ToLower() == "active")
                {
                    query = query.Where(u => u.Status == UserStatus.Active);
                }
                else if (status.ToLower() == "inactive")
                {
                    query = query.Where(u => u.Status == UserStatus.Inactive);
                }
            }

            // Get total count
            var totalCount = await query.CountAsync();
            var activeCount = await _context.Users.Where(u => u.Role == UserRole.B2C && u.Status == UserStatus.Active).CountAsync();
            
            // Calculate stats
            var totalOrders = await _context.Orders.CountAsync();
            var totalSpent = await _context.Orders
                .Where(o => o.Status != OrderStatus.Cancelled)
                .SumAsync(o => o.TotalAmount);

            // Get unique cities from addresses
            var allMembers = await _context.Users.Where(u => u.Role == UserRole.B2C && u.Address != null).ToListAsync();
            var cities = allMembers
                .Select(u => ExtractCity(u.Address))
                .Where(c => !string.IsNullOrEmpty(c))
                .Distinct()
                .OrderBy(c => c)
                .ToList();

            // Get members with pagination
            var pageSize = 10;
            var members = await query
                .OrderByDescending(u => u.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(u => new MemberViewModel
                {
                    Id = u.Id,
                    MemberId = "M" + u.Id.ToString().PadLeft(5, '0'),
                    FullName = u.FirstName + " " + u.LastName,
                    Email = u.Email,
                    Phone = u.Phone ?? "-",
                    Address = u.Address ?? "-",
                    UserStatus = u.Status,
                    CreatedAt = u.CreatedAt,
                    LastLogin = u.UpdatedAt ?? u.CreatedAt, // Son giriş tarihi
                    TotalOrders = _context.Orders.Count(o => o.UserId == u.Id),
                    TotalSpent = _context.Orders
                        .Where(o => o.UserId == u.Id && o.Status != OrderStatus.Cancelled)
                        .Sum(o => o.TotalAmount)
                })
                .ToListAsync();

            ViewBag.TotalCount = totalCount;
            ViewBag.ActiveCount = activeCount;
            ViewBag.TotalOrders = totalOrders;
            ViewBag.AverageLTV = totalCount > 0 ? totalSpent / totalCount : 0;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalCount / pageSize);
            ViewBag.MemberId = memberId;
            ViewBag.Name = name;
            ViewBag.Email = email;
            ViewBag.City = city;
            ViewBag.Cities = cities;
            ViewBag.Status = status;

            return View(members);
        }

        private string? ExtractCity(string? address)
        {
            if (string.IsNullOrEmpty(address)) return null;
            
            // Adres formatı: "..., İl" veya sadece şehir adı
            var parts = address.Split(',');
            if (parts.Length >= 2)
            {
                return parts[^1].Trim(); // Son kısım genellikle şehir
            }
            return address.Trim();
        }

        public async Task<IActionResult> Details(int id)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == id && u.Role == UserRole.B2C);

            if (user == null)
            {
                return NotFound();
            }

            var orders = await _context.Orders
                .Where(o => o.UserId == id)
                .OrderByDescending(o => o.CreatedAt)
                .Take(10)
                .ToListAsync();

            var totalOrders = await _context.Orders.CountAsync(o => o.UserId == id);
            var totalSpent = await _context.Orders
                .Where(o => o.UserId == id && o.Status != OrderStatus.Cancelled)
                .SumAsync(o => o.TotalAmount);
            var averageOrderValue = totalOrders > 0 ? totalSpent / totalOrders : 0;

            // Generate activities based on orders and login
            var activities = new List<ActivityViewModel>();
            
            // Add last login activity
            if (user.UpdatedAt.HasValue)
            {
                activities.Add(new ActivityViewModel
                {
                    Type = "login",
                    Title = "Son Giriş",
                    Description = "Sisteme giriş yaptı",
                    Timestamp = user.UpdatedAt.Value
                });
            }

            // Add order activities
            foreach (var order in orders.Take(4))
            {
                activities.Add(new ActivityViewModel
                {
                    Type = "order",
                    Title = "Sipariş Verdi",
                    Description = order.OrderNumber,
                    Timestamp = order.CreatedAt
                });
            }

            // Sort by timestamp descending
            activities = activities.OrderByDescending(a => a.Timestamp).Take(5).ToList();

            var viewModel = new MemberDetailViewModel
            {
                Id = user.Id,
                MemberId = "M" + user.Id.ToString().PadLeft(5, '0'),
                FullName = user.FirstName + " " + user.LastName,
                Email = user.Email,
                Phone = user.Phone ?? "-",
                Address = user.Address ?? "-",
                UserStatus = user.Status,
                CreatedAt = user.CreatedAt,
                TotalOrders = totalOrders,
                TotalSpent = totalSpent,
                AverageOrderValue = averageOrderValue,
                Activities = activities,
                RecentOrders = orders.Select(o => new OrderSummaryViewModel
                {
                    Id = o.Id,
                    OrderNumber = o.OrderNumber,
                    CreatedAt = o.CreatedAt,
                    TotalAmount = o.TotalAmount,
                    Status = o.Status
                }).ToList()
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            user.Status = user.Status == UserStatus.Active ? UserStatus.Inactive : UserStatus.Active;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Details), new { id });
        }

        // Export to Excel (CSV Format - Excel Compatible)
        [HttpGet]
        public async Task<IActionResult> Export()
        {
            var members = await _context.Users
                .Where(u => u.Role == UserRole.B2C)
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync();

            var csv = new System.Text.StringBuilder();
            // BOM for UTF-8 Excel compatibility
            csv.Append('\uFEFF');
            csv.AppendLine("Üye ID\tAd Soyad\tE-posta\tTelefon\tAdres\tDurum\tKayıt Tarihi");

            foreach (var m in members)
            {
                csv.AppendLine($"M{m.Id:D5}\t{m.FirstName} {m.LastName}\t{m.Email}\t{m.Phone ?? "-"}\t{m.Address ?? "-"}\t{(m.Status == UserStatus.Active ? "Aktif" : "Pasif")}\t{m.CreatedAt:dd.MM.yyyy}");
            }

            var bytes = System.Text.Encoding.UTF8.GetBytes(csv.ToString());
            return File(bytes, "application/vnd.ms-excel", $"Uyeler_{DateTime.Now:yyyyMMdd}.xls");
        }

        // Import from CSV
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Import(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                TempData["ErrorMessage"] = "Lütfen bir dosya seçin.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                using var reader = new System.IO.StreamReader(file.OpenReadStream());
                var content = await reader.ReadToEndAsync();
                var lines = content.Split('\n').Skip(1); // Skip header
                var importCount = 0;

                foreach (var line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    
                    var parts = line.Split(',');
                    if (parts.Length >= 3)
                    {
                        var nameParts = parts[1].Trim().Split(' ');
                        var email = parts[2].Trim();

                        // Check if email already exists
                        if (await _context.Users.AnyAsync(u => u.Email == email))
                            continue;

                        var user = new User
                        {
                            FirstName = nameParts.FirstOrDefault() ?? "İsim",
                            LastName = nameParts.Length > 1 ? string.Join(" ", nameParts.Skip(1)) : "Soyisim",
                            Email = email,
                            Phone = parts.Length > 3 ? parts[3].Trim() : null,
                            Address = parts.Length > 4 ? parts[4].Trim() : null,
                            PasswordHash = "temp123",
                            Role = UserRole.B2C,
                            Status = UserStatus.Active,
                            CreatedAt = DateTime.Now,
                            IsActive = true
                        };

                        _context.Users.Add(user);
                        importCount++;
                    }
                }

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"{importCount} üye başarıyla içe aktarıldı!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"İçe aktarma hatası: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        // Create new member
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string firstName, string lastName, string email, string? phone, string? address)
        {
            try
            {
                // Check if email already exists
                if (await _context.Users.AnyAsync(u => u.Email == email))
                {
                    return Json(new { success = false, message = "Bu e-posta adresi zaten kullanılıyor." });
                }

                var user = new User
                {
                    FirstName = firstName,
                    LastName = lastName,
                    Email = email,
                    Phone = phone,
                    Address = address,
                    PasswordHash = "temp123",
                    Role = UserRole.B2C,
                    Status = UserStatus.Active,
                    CreatedAt = DateTime.Now,
                    IsActive = true
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Yeni üye başarıyla oluşturuldu!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Bir hata oluştu: " + ex.Message });
            }
        }

        // Update member contact info
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(int id, string? phone, string? address)
        {
            try
            {
                var user = await _context.Users.FindAsync(id);
                if (user == null)
                {
                    return Json(new { success = false, message = "Üye bulunamadı." });
                }

                user.Phone = phone;
                user.Address = address;
                user.UpdatedAt = DateTime.Now;
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Bilgiler güncellendi!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Bir hata oluştu: " + ex.Message });
            }
        }

        // Reset password
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(int id)
        {
            try
            {
                var user = await _context.Users.FindAsync(id);
                if (user == null)
                {
                    return Json(new { success = false, message = "Üye bulunamadı." });
                }

                user.PasswordHash = "temp123";
                user.UpdatedAt = DateTime.Now;
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Şifre sıfırlandı!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Bir hata oluştu: " + ex.Message });
            }
        }
    }

    // ViewModels
    public class MemberViewModel
    {
        public int Id { get; set; }
        public string MemberId { get; set; } = "";
        public string FullName { get; set; } = "";
        public string Email { get; set; } = "";
        public string Phone { get; set; } = "";
        public string Address { get; set; } = "";
        public UserStatus UserStatus { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastLogin { get; set; } // Son giriş tarihi
        public int TotalOrders { get; set; }
        public decimal TotalSpent { get; set; }
    }

    public class MemberDetailViewModel : MemberViewModel
    {
        public decimal AverageOrderValue { get; set; }
        public List<OrderSummaryViewModel> RecentOrders { get; set; } = new();
        public List<ActivityViewModel> Activities { get; set; } = new();
    }

    public class OrderSummaryViewModel
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; } = "";
        public DateTime CreatedAt { get; set; }
        public decimal TotalAmount { get; set; }
        public OrderStatus Status { get; set; }
    }
    
    public class ActivityViewModel
    {
        public string Type { get; set; } = ""; // login, order, cart, review
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public DateTime Timestamp { get; set; }
    }
}
