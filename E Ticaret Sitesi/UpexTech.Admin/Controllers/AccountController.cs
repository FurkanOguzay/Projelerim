using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using UpexTech.Admin.Models;
using UpexTech.Business.Services;
using UpexTech.Entity;

namespace UpexTech.Admin.Controllers
{
    [AllowAnonymous]
    public class AccountController : Controller
    {
        private readonly IAdminUserService _adminUserService;

        public AccountController(IAdminUserService adminUserService)
        {
            _adminUserService = adminUserService;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Dashboard");
            }
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var adminUser = await _adminUserService.AuthenticateAsync(model.Email, model.Password);

            if (adminUser == null)
            {
                ModelState.AddModelError(string.Empty, "E-posta veya şifre hatalı!");
                return View(model);
            }

            if (!adminUser.IsActive)
            {
                ModelState.AddModelError(string.Empty, "Hesabınız devre dışı bırakılmıştır.");
                return View(model);
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, adminUser.Id.ToString()),
                new Claim(ClaimTypes.Name, adminUser.FullName),
                new Claim(ClaimTypes.Email, adminUser.Email),
                new Claim(ClaimTypes.Role, adminUser.Role.Name),
                new Claim("RoleId", adminUser.RoleId.ToString()),
                new Claim("Permissions", ((int)adminUser.Role.Permissions).ToString())
            };

            var claimsIdentity = new ClaimsIdentity(claims, "AdminCookies");
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = model.RememberMe,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(24)
            };

            await HttpContext.SignInAsync("AdminCookies", new ClaimsPrincipal(claimsIdentity), authProperties);

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "Dashboard");
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("AdminCookies");
            return RedirectToAction("Login");
        }

        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
