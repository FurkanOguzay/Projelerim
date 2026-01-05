using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace UpexTech.Web.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "E-posta gereklidir")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz")]
        [Display(Name = "E-posta")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Şifre gereklidir")]
        [DataType(DataType.Password)]
        [Display(Name = "Şifre")]
        public string Password { get; set; } = string.Empty;

        public bool RememberMe { get; set; }

        public string? ReturnUrl { get; set; }
        public string? ErrorMessage { get; set; }
    }

    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Ad gereklidir")]
        [Display(Name = "Ad")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Soyad gereklidir")]
        [Display(Name = "Soyad")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "E-posta gereklidir")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz")]
        [Display(Name = "E-posta")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Şifre gereklidir")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Şifre en az 6 karakter olmalıdır")]
        [DataType(DataType.Password)]
        [Display(Name = "Şifre")]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Şifre Tekrar")]
        [Compare("Password", ErrorMessage = "Şifreler eşleşmiyor")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Display(Name = "Telefon")]
        public string? Phone { get; set; }

        [Display(Name = "Bayi olarak kayıt ol")]
        public bool IsDealer { get; set; }

        // Bayi alanları
        [Display(Name = "Firma Adı")]
        public string? CompanyName { get; set; }

        [Display(Name = "Vergi Numarası")]
        public string? TaxNumber { get; set; }

        [Display(Name = "Adres")]
        public string? Address { get; set; }

        // Kurum Evrakları - Dosya yükleme
        [Display(Name = "Kurum Evrakları")]
        public IFormFileCollection? CompanyDocuments { get; set; }

        public string? ErrorMessage { get; set; }
    }
}
