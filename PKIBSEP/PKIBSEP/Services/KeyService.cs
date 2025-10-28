using PKIBSEP.Dtos.Keys;
using PKIBSEP.Enums;
using PKIBSEP.Interfaces;

namespace PKIBSEP.Services
{
    public class KeyService : IKeyService
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger<KeyService> _logger;

        public KeyService(IUserRepository userRepository, ILogger<KeyService> logger)
        {
            _userRepository = userRepository;
            _logger = logger;
        }

        public async Task<(bool success, string message)> SavePublicKeyAsync(int userId, string publicKeyPem)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);

                if (user == null)
                {
                    return (false, "Korisnik nije pronađen");
                }

                if (user.Role != UserRole.RegularUser)
                {
                    return (false, "Samo obični korisnici mogu koristiti password manager");
                }

                if (!IsValidPemFormat(publicKeyPem))
                {
                    return (false, "Neispravan format javnog ključa");
                }

                user.PublicKeyPem = publicKeyPem;
                user.KeyGeneratedAt = DateTime.UtcNow;

                await _userRepository.UpdateAsync(user);

                _logger.LogInformation("Javni ključ sačuvan za korisnika {UserId}", userId);

                return (true, "Javni ključ je uspešno sačuvan");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška prilikom čuvanja javnog ključa za korisnika {UserId}", userId);
                return (false, "Greška prilikom čuvanja javnog ključa");
            }
        }

        public async Task<PublicKeyResponseDto?> GetPublicKeyAsync(int userId)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);

                if (user == null)
                {
                    return null;
                }

                return new PublicKeyResponseDto
                {
                    PublicKeyPem = user.PublicKeyPem,
                    KeyGeneratedAt = user.KeyGeneratedAt,
                    HasKey = !string.IsNullOrEmpty(user.PublicKeyPem)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška prilikom preuzimanja javnog ključa za korisnika {UserId}", userId);
                return null;
            }
        }

        public async Task<UserPublicKeyDto?> GetUserPublicKeyAsync(int requestingUserId, int targetUserId)
        {
            try
            {
                var requestingUser = await _userRepository.GetByIdAsync(requestingUserId);
                if (requestingUser == null || requestingUser.Role != UserRole.RegularUser)
                {
                    return null;
                }

                var targetUser = await _userRepository.GetByIdAsync(targetUserId);

                if (targetUser == null)
                {
                    return null;
                }

                // Vraćamo javni ključ samo ako je target korisnik RegularUser
                if (targetUser.Role != UserRole.RegularUser)
                {
                    return null;
                }

                return new UserPublicKeyDto
                {
                    UserId = targetUser.Id,
                    Email = targetUser.Email,
                    PublicKeyPem = targetUser.PublicKeyPem,
                    HasKey = !string.IsNullOrEmpty(targetUser.PublicKeyPem)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška prilikom preuzimanja javnog ključa korisnika {TargetUserId}", targetUserId);
                return null;
            }
        }

        private bool IsValidPemFormat(string pem)
        {
            if (string.IsNullOrWhiteSpace(pem))
            {
                return false;
            }

            return pem.Contains("-----BEGIN PUBLIC KEY-----") &&
                   pem.Contains("-----END PUBLIC KEY-----");
        }
    }
}
