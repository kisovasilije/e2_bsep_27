using System.Text.RegularExpressions;
using PKIBSEP.Dtos;

namespace PKIBSEP.Services
{
    public class PasswordValidator
    {
        private const int MinLength = 8;
        private const int MaxLength = 64;

        public static PasswordStrengthDto EvaluateStrength(string password)
        {
            var result = new PasswordStrengthDto
            {
                Suggestions = new List<string>()
            };

            if (string.IsNullOrWhiteSpace(password))
            {
                result.Score = 0;
                result.Strength = "Very Weak";
                result.MeetsMinimumRequirements = false;
                result.Suggestions.Add("Lozinka je obavezna");
                return result;
            }

            int score = 0;
            var suggestions = new List<string>();

            // Length check
            if (password.Length < MinLength)
            {
                suggestions.Add($"Lozinka mora imati najmanje {MinLength} karaktera");
            }
            else if (password.Length >= MinLength && password.Length < 12)
            {
                score += 1;
                suggestions.Add("Razmotrite upotrebu duže lozinke (12+ karaktera)");
            }
            else if (password.Length >= 12 && password.Length < 16)
            {
                score += 2;
            }
            else if (password.Length >= 16)
            {
                score += 3;
            }

            if (password.Length > MaxLength)
            {
                suggestions.Add($"Lozinka ne sme biti duža od {MaxLength} karaktera");
                result.Score = 0;
                result.Strength = "Invalid";
                result.MeetsMinimumRequirements = false;
                result.Suggestions = suggestions;
                return result;
            }

            // Uppercase check
            if (Regex.IsMatch(password, @"[A-Z]"))
            {
                score += 1;
            }
            else
            {
                suggestions.Add("Dodajte najmanje jedno veliko slovo");
            }

            // Lowercase check
            if (Regex.IsMatch(password, @"[a-z]"))
            {
                score += 1;
            }
            else
            {
                suggestions.Add("Dodajte najmanje jedno malo slovo");
            }

            // Digit check
            if (Regex.IsMatch(password, @"\d"))
            {
                score += 1;
            }
            else
            {
                suggestions.Add("Dodajte najmanje jednu cifru");
            }

            // Special character check
            if (Regex.IsMatch(password, @"[!@#$%^&*()_+\-=\[\]{};':""\\|,.<>\/?]"))
            {
                score += 2;
            }
            else
            {
                suggestions.Add("Dodajte najmanje jedan specijalni karakter (!@#$%^&*...)");
            }

            // Check for common patterns
            if (Regex.IsMatch(password, @"(012|123|234|345|456|567|678|789|890|abc|bcd|cde|def|efg|fgh|ghi|hij|ijk|jkl|klm|lmn|mno|nop|opq|pqr|qrs|rst|stu|tuv|uvw|vwx|wxy|xyz)", RegexOptions.IgnoreCase))
            {
                score -= 1;
                suggestions.Add("Izbegavajte sekvencijalne karaktere (123, abc, itd.)");
            }

            // Check for repeated characters
            if (Regex.IsMatch(password, @"(.)\1{2,}"))
            {
                score -= 1;
                suggestions.Add("Izbegavajte ponavljanje istih karaktera (aaa, 111, itd.)");
            }

            // Common weak passwords
            var commonPasswords = new[] { "password", "lozinka", "admin", "user", "12345678", "qwerty", "letmein" };
            if (commonPasswords.Any(p => password.ToLower().Contains(p)))
            {
                score = 0;
                suggestions.Add("Lozinka je previše česta i lako se pogađa");
            }

            // Normalize score to 0-4
            score = Math.Max(0, Math.Min(4, score));

            result.Score = score;
            result.Strength = score switch
            {
                0 => "Very Weak",
                1 => "Weak",
                2 => "Medium",
                3 => "Strong",
                4 => "Very Strong",
                _ => "Unknown"
            };

            // Minimum requirements: at least 8 chars, 1 upper, 1 lower, 1 digit, 1 special
            result.MeetsMinimumRequirements = password.Length >= MinLength &&
                                               Regex.IsMatch(password, @"[A-Z]") &&
                                               Regex.IsMatch(password, @"[a-z]") &&
                                               Regex.IsMatch(password, @"\d") &&
                                               Regex.IsMatch(password, @"[!@#$%^&*()_+\-=\[\]{};':""\\|,.<>\/?]");

            result.Suggestions = suggestions;

            return result;
        }

        public static bool IsValid(string password)
        {
            var strength = EvaluateStrength(password);
            return strength.MeetsMinimumRequirements;
        }
    }
}
