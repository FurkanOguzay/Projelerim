using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using UpexTech.Business.Services;
using UpexTech.Entity;
using UpexTech.Web.Models;

namespace UpexTech.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUserService _userService;

        public AccountController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null, string? type = null)
        {
            if (User.Identity?.IsAuthenticated ?? false)
            {
                return RedirectToAction("Index", "Home");
            }

            ViewBag.LoginType = type;
            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model, string? loginType = null)
        {
            ViewBag.LoginType = loginType;
            
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userService.AuthenticateAsync(model.Email, model.Password);

            if (user == null)
            {
                model.ErrorMessage = "Hatalı e-posta veya şifre.";
                return View(model);
            }

            // Giriş tipi kontrolü - Bayi girişi için B2B veya Admin olmalı
            if (loginType == "dealer" && user.Role == UserRole.B2C)
            {
                model.ErrorMessage = "Bu hesap bireysel müşteri hesabıdır. Lütfen bireysel giriş yapın.";
                return View(model);
            }

            // Bireysel giriş için B2C olmalı (Admin hariç)
            if (loginType == "customer" && user.Role == UserRole.B2B)
            {
                model.ErrorMessage = "Bu hesap bayi hesabıdır. Lütfen bayi girişi yapın.";
                return View(model);
            }

            if (user.Status == UserStatus.Pending)
            {
                if (user.Role == UserRole.B2B)
                {
                    model.ErrorMessage = "Bayi hesabınız onay aşamasındadır. Başvurunuz incelendikten sonra e-posta ile bilgilendirileceksiniz. Lütfen onaylandıktan sonra tekrar deneyin.";
                }
                else
                {
                    model.ErrorMessage = "Hesabınız onay aşamasındadır. Lütfen onaylandıktan sonra tekrar deneyin.";
                }
                return View(model);
            }

            if (user.Status != UserStatus.Active)
            {
                model.ErrorMessage = "Hesabınız aktif değil.";
                return View(model);
            }

            // Create claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = model.RememberMe,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(24)
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            // Redirect based on role
            if (user.Role == UserRole.Admin)
            {
                // Admin kullanıcıları bu siteden giriş yapamaz - ayrı admin panelini kullanmalılar
                model.ErrorMessage = "Yönetici girişi için lütfen Admin Paneli'ni kullanın (https://localhost:5001)";
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                return View(model);
            }

            // Show campaign popup after login
            TempData["ShowCampaignPopup"] = true;

            if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            {
                return Redirect(model.ReturnUrl);
            }

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult Register()
        {
            if (User.Identity?.IsAuthenticated ?? false)
            {
                return RedirectToAction("Index", "Home");
            }

            return View(new RegisterViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Şifre güvenlik validasyonu
            var passwordValidation = UpexTech.Business.Helpers.PasswordValidator.Validate(model.Password);
            if (!passwordValidation.IsValid)
            {
                model.ErrorMessage = passwordValidation.ErrorMessage;
                return View(model);
            }

            // Check if email exists
            if (await _userService.EmailExistsAsync(model.Email))
            {
                model.ErrorMessage = "Bu e-posta adresi zaten kullanılıyor.";
                return View(model);
            }

            // Validate dealer fields
            if (model.IsDealer)
            {
                if (string.IsNullOrEmpty(model.CompanyName))
                {
                    model.ErrorMessage = "Bayi kaydı için firma adı gereklidir.";
                    return View(model);
                }
                if (string.IsNullOrEmpty(model.TaxNumber))
                {
                    model.ErrorMessage = "Bayi kaydı için vergi numarası gereklidir.";
                    return View(model);
                }
            }

            // Handle file upload for dealer registration
            var uploadedDocumentPaths = new List<string>();
            if (model.IsDealer && model.CompanyDocuments != null && model.CompanyDocuments.Count > 0)
            {
                var allowedExtensions = new[] { ".pdf", ".jpg", ".jpeg", ".png" };
                const long maxFileSize = 5 * 1024 * 1024; // 5MB
                
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "dealer-documents");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                foreach (var file in model.CompanyDocuments)
                {
                    if (file.Length > 0)
                    {
                        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                        
                        // Validate file type
                        if (!allowedExtensions.Contains(extension))
                        {
                            model.ErrorMessage = $"Geçersiz dosya türü: {file.FileName}. Sadece PDF, JPG, PNG dosyaları yüklenebilir.";
                            return View(model);
                        }
                        
                        // Validate file size
                        if (file.Length > maxFileSize)
                        {
                            model.ErrorMessage = $"Dosya çok büyük: {file.FileName}. Maksimum dosya boyutu 5MB'dır.";
                            return View(model);
                        }
                        
                        // Generate unique filename
                        var uniqueFileName = $"{Guid.NewGuid()}{extension}";
                        var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                        
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }
                        
                        uploadedDocumentPaths.Add($"/uploads/dealer-documents/{uniqueFileName}");
                    }
                }
            }

            var user = new User
            {
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,
                Phone = model.Phone,
                Role = model.IsDealer ? UserRole.B2B : UserRole.B2C,
                CompanyName = model.CompanyName,
                TaxNumber = model.TaxNumber,
                Address = model.Address
            };

            // Note: uploadedDocumentPaths can be saved to User entity if CompanyDocuments property exists
            // user.CompanyDocuments = string.Join(";", uploadedDocumentPaths);

            await _userService.RegisterAsync(user, model.Password);

            if (model.IsDealer)
            {
                TempData["SuccessMessage"] = "Bayi başvurunuz alınmıştır. Onay sonrası giriş yapabilirsiniz.";
            }
            else
            {
                TempData["SuccessMessage"] = "Kayıt başarılı. Giriş yapabilirsiniz.";
            }

            return RedirectToAction("Login");
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            if (User.Identity?.IsAuthenticated ?? false)
            {
                return RedirectToAction("Index", "Home");
            }

            return View(new ForgotPasswordViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // E-posta adresinin sistemde kayıtlı olup olmadığını kontrol et
            var emailExists = await _userService.EmailExistsAsync(model.Email);
            
            if (emailExists)
            {
                // Gerçek bir uygulamada burada:
                // 1. Benzersiz bir token oluşturulur
                // 2. Token veritabanına kaydedilir
                // 3. Kullanıcıya şifre sıfırlama linki içeren e-posta gönderilir
                
                // Demo amaçlı başarı mesajı göster
                model.SuccessMessage = "Şifre sıfırlama bağlantısı e-posta adresinize gönderildi. Lütfen gelen kutunuzu kontrol edin.";
            }
            else
            {
                // Güvenlik nedeniyle aynı mesajı göster (e-posta var mı yok mu belli etme)
                model.SuccessMessage = "Şifre sıfırlama bağlantısı e-posta adresinize gönderildi. Lütfen gelen kutunuzu kontrol edin.";
            }

            return View(model);
        }

        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
