using System.Text.RegularExpressions;

namespace UpexTech.Business.Helpers
{
    public static class PasswordValidator
    {
        // Şifre kuralları
        private const int MinLength = 8;
        
        // Regex patterns
        private static readonly Regex UppercaseRegex = new Regex(@"[A-Z]", RegexOptions.Compiled);
        private static readonly Regex LowercaseRegex = new Regex(@"[a-z]", RegexOptions.Compiled);
        private static readonly Regex DigitRegex = new Regex(@"[0-9]", RegexOptions.Compiled);
        private static readonly Regex SpecialCharRegex = new Regex(@"[!@#$%^&*()_+\-=\[\]{};':""\\|,.<>\/?]", RegexOptions.Compiled);

        /// <summary>
        /// Şifrenin tüm güvenlik gereksinimlerini karşılayıp karşılamadığını kontrol eder
        /// </summary>
        public static (bool IsValid, string ErrorMessage) Validate(string password)
        {
            if (string.IsNullOrEmpty(password))
            {
                return (false, "Şifre boş olamaz.");
            }

            if (password.Length < MinLength)
            {
                return (false, $"Şifre en az {MinLength} karakter olmalıdır.");
            }

            if (!UppercaseRegex.IsMatch(password))
            {
                return (false, "Şifre en az 1 büyük harf (A-Z) içermelidir.");
            }

            if (!LowercaseRegex.IsMatch(password))
            {
                return (false, "Şifre en az 1 küçük harf (a-z) içermelidir.");
            }

            if (!DigitRegex.IsMatch(password))
            {
                return (false, "Şifre en az 1 rakam (0-9) içermelidir.");
            }

            if (!SpecialCharRegex.IsMatch(password))
            {
                return (false, "Şifre en az 1 özel karakter (!, @, #, $, %, vb.) içermelidir.");
            }

            return (true, string.Empty);
        }

        /// <summary>
        /// Şifrenin tüm kuralları sağlayıp sağlamadığını döndürür
        /// </summary>
        public static bool IsValid(string password)
        {
            return Validate(password).IsValid;
        }

        /// <summary>
        /// Tüm şifre gereksinimlerini listeler
        /// </summary>
        public static string GetRequirements()
        {
            return $"Şifre en az {MinLength} karakter, 1 büyük harf, 1 küçük harf, 1 rakam ve 1 özel karakter içermelidir.";
        }
    }
}
