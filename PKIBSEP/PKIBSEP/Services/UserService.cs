using System.Security.Cryptography;
using PKIBSEP.Dtos;
using PKIBSEP.Enums;
using PKIBSEP.Interfaces;
using PKIBSEP.Models;

namespace PKIBSEP.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<UserService> _logger;

        public UserService(
            IUserRepository userRepository,
            IEmailService emailService,
            IConfiguration configuration,
            ILogger<UserService> logger)
        {
            _userRepository = userRepository;
            _emailService = emailService;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<RegisterResponseDto> RegisterAsync(RegisterDto registerDto)
        {
            try
            {
                // Validate password strength
                var passwordStrength = PasswordValidator.EvaluateStrength(registerDto.Password);
                if (!passwordStrength.MeetsMinimumRequirements)
                {
                    _logger.LogWarning("Pokušaj registracije sa slabom lozinkom za email: {Email}", registerDto.Email);
                    return new RegisterResponseDto
                    {
                        Success = false,
                        Message = "Lozinka ne ispunjava minimalne zahteve: " + string.Join(", ", passwordStrength.Suggestions)
                    };
                }

                // Check if user already exists
                if (await _userRepository.EmailExistsAsync(registerDto.Email))
                {
                    _logger.LogWarning("Pokušaj registracije sa već postojećim email-om: {Email}", registerDto.Email);
                    return new RegisterResponseDto
                    {
                        Success = false,
                        Message = "Korisnik sa ovom email adresom već postoji"
                    };
                }

                // Generate email confirmation token
                var confirmationToken = GenerateSecureToken();
                var tokenExpiry = DateTime.UtcNow.AddHours(
                    double.Parse(_configuration["AppSettings:EmailConfirmationTokenExpiryHours"]!));

                // Create new user
                var user = new User
                {
                    Email = registerDto.Email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password),
                    Username = registerDto.Username,
                    Organization = registerDto.Organization,
                    Role = UserRole.RegularUser,
                    IsEmailConfirmed = false,
                    EmailConfirmationToken = confirmationToken,
                    EmailConfirmationTokenExpiry = tokenExpiry,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };

                await _userRepository.CreateAsync(user);

                // Send confirmation email
                var frontendUrl = _configuration["AppSettings:FrontendUrl"];
                var confirmationLink = $"{frontendUrl}/confirm-email?token={confirmationToken}";

                await _emailService.SendEmailConfirmationAsync(user.Email, confirmationLink);

                _logger.LogInformation("Novi korisnik registrovan: {Email}, ID: {UserId}", user.Email, user.Id);

                return new RegisterResponseDto
                {
                    Success = true,
                    Message = "Registracija uspešna! Proverite svoj email za aktivacioni link.",
                    Email = user.Email
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška prilikom registracije korisnika: {Email}", registerDto.Email);
                return new RegisterResponseDto
                {
                    Success = false,
                    Message = "Došlo je do greške prilikom registracije. Molimo pokušajte ponovo."
                };
            }
        }

        public async Task<(bool success, string message)> ConfirmEmailAsync(string token)
        {
            try
            {
                if (string.IsNullOrEmpty(token))
                {
                    return (false, "Token je obavezan");
                }

                var user = await _userRepository.GetByEmailConfirmationTokenAsync(token);

                if (user == null)
                {
                    _logger.LogWarning("Pokušaj aktivacije sa nevalidnim tokenom");
                    return (false, "Nevalidan aktivacioni link");
                }

                if (user.IsEmailConfirmed)
                {
                    _logger.LogInformation("Pokušaj aktivacije već aktiviranog naloga: {Email}", user.Email);
                    return (false, "Nalog je već aktiviran");
                }

                if (user.EmailConfirmationTokenExpiry < DateTime.UtcNow)
                {
                    _logger.LogWarning("Pokušaj aktivacije sa isteklim tokenom za korisnika: {Email}", user.Email);
                    return (false, "Aktivacioni link je istekao");
                }

                user.IsEmailConfirmed = true;
                user.EmailConfirmationToken = null;
                user.EmailConfirmationTokenExpiry = null;

                await _userRepository.UpdateAsync(user);

                _logger.LogInformation("Nalog uspešno aktiviran: {Email}, ID: {UserId}", user.Email, user.Id);

                return (true, "Nalog je uspešno aktiviran! Možete se prijaviti.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška prilikom aktivacije naloga");
                return (false, "Došlo je do greške prilikom aktivacije naloga");
            }
        }

        public async Task<(bool success, string message)> ForgotPasswordAsync(string email)
        {
            try
            {
                var user = await _userRepository.GetByEmailAsync(email);

                if (user == null)
                {
                    // Ne otkrivamo da korisnik ne postoji iz bezbednosnih razloga
                    _logger.LogWarning("Pokušaj oporavka naloga za nepostojeći email: {Email}", email);
                    return (true, "Ako nalog sa tom email adresom postoji, poslat je link za resetovanje lozinke.");
                }

                if (!user.IsEmailConfirmed)
                {
                    _logger.LogWarning("Pokušaj oporavka naloga koji nije aktiviran: {Email}", email);
                    return (false, "Nalog nije aktiviran. Molimo aktivirajte nalog pre nego što resetujete lozinku.");
                }

                // Generate password reset token
                var resetToken = GenerateSecureToken();
                var tokenExpiry = DateTime.UtcNow.AddHours(
                    double.Parse(_configuration["AppSettings:PasswordResetTokenExpiryHours"]!));

                user.PasswordResetToken = resetToken;
                user.PasswordResetTokenExpiry = tokenExpiry;

                await _userRepository.UpdateAsync(user);

                // Send password reset email
                var frontendUrl = _configuration["AppSettings:FrontendUrl"];
                var resetLink = $"{frontendUrl}/reset-password?token={resetToken}";

                await _emailService.SendPasswordResetAsync(user.Email, resetLink);

                _logger.LogInformation("Zahtev za resetovanje lozinke poslat za korisnika: {Email}", user.Email);

                return (true, "Ako nalog sa tom email adresom postoji, poslat je link za resetovanje lozinke.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška prilikom slanja zahteva za resetovanje lozinke: {Email}", email);
                return (false, "Došlo je do greške prilikom slanja zahteva za resetovanje lozinke.");
            }
        }

        public async Task<(bool success, string message)> ResetPasswordAsync(string token, string newPassword)
        {
            try
            {
                if (string.IsNullOrEmpty(token))
                {
                    return (false, "Token je obavezan");
                }

                // Validate password strength
                var passwordStrength = PasswordValidator.EvaluateStrength(newPassword);
                if (!passwordStrength.MeetsMinimumRequirements)
                {
                    _logger.LogWarning("Pokušaj resetovanja lozinke sa slabom lozinkom");
                    return (false, "Lozinka ne ispunjava minimalne zahteve: " + string.Join(", ", passwordStrength.Suggestions));
                }

                var user = await _userRepository.GetByPasswordResetTokenAsync(token);

                if (user == null)
                {
                    _logger.LogWarning("Pokušaj resetovanja lozinke sa nevalidnim tokenom");
                    return (false, "Nevalidan link za resetovanje lozinke");
                }

                if (user.PasswordResetTokenExpiry < DateTime.UtcNow)
                {
                    _logger.LogWarning("Pokušaj resetovanja lozinke sa isteklim tokenom za korisnika: {Email}", user.Email);
                    return (false, "Link za resetovanje lozinke je istekao");
                }

                // Update password
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
                user.PasswordResetToken = null;
                user.PasswordResetTokenExpiry = null;

                await _userRepository.UpdateAsync(user);

                _logger.LogInformation("Lozinka uspešno resetovana za korisnika: {Email}, ID: {UserId}", user.Email, user.Id);

                return (true, "Lozinka je uspešno resetovana. Možete se prijaviti sa novom lozinkom.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška prilikom resetovanja lozinke");
                return (false, "Došlo je do greške prilikom resetovanja lozinke");
            }
        }

        public PasswordStrengthDto CheckPasswordStrength(string password)
        {
            return PasswordValidator.EvaluateStrength(password);
        }

        private string GenerateSecureToken()
        {
            var randomBytes = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
            }
            return Convert.ToBase64String(randomBytes)
                .Replace("+", "-")
                .Replace("/", "_")
                .Replace("=", "");
        }
    }
}
