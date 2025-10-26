using PKIBSEP.Dtos.PasswordManager;
using PKIBSEP.Enums;
using PKIBSEP.Interfaces;
using PKIBSEP.Interfaces.Repository;
using PKIBSEP.Models;

namespace PKIBSEP.Services
{
    public class PasswordService : IPasswordService
    {
        private readonly IPasswordRepository _passwordRepository;
        private readonly IUserRepository _userRepository;
        private readonly ILogger<PasswordService> _logger;

        public PasswordService(
            IPasswordRepository passwordRepository,
            IUserRepository userRepository,
            ILogger<PasswordService> logger)
        {
            _passwordRepository = passwordRepository;
            _userRepository = userRepository;
            _logger = logger;
        }

        public async Task<List<PasswordEntryDto>> GetUserPasswordsAsync(int userId)
        {
            try
            {
                var passwords = await _passwordRepository.GetUserPasswordsAsync(userId);

                return passwords.Select(p => MapToDto(p, userId)).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška pri preuzimanju lozinki za korisnika {UserId}", userId);
                return new List<PasswordEntryDto>();
            }
        }

        public async Task<PasswordEntryDto?> GetPasswordByIdAsync(int id, int userId)
        {
            try
            {
                var password = await _passwordRepository.GetByIdAsync(id);

                if (password == null)
                {
                    return null;
                }

                // Provera da li korisnik ima pristup (owner ili share)
                if (password.OwnerId != userId)
                {
                    var share = await _passwordRepository.GetPasswordShareAsync(id, userId);
                    if (share == null)
                    {
                        return null; // Korisnik nema pristup
                    }
                }

                return MapToDto(password, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška pri preuzimanju lozinke {Id}", id);
                return null;
            }
        }

        public async Task<(bool success, string message, PasswordEntryDto? entry)> CreatePasswordAsync(int userId, SavePasswordDto dto)
        {
            try
            {
                // Provera da je korisnik RegularUser
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    return (false, "Korisnik nije pronađen", null);
                }

                if (user.Role != UserRole.RegularUser)
                {
                    return (false, "Samo obični korisnici mogu koristiti password manager", null);
                }

                // Provera da korisnik ima javni ključ
                if (string.IsNullOrEmpty(user.PublicKeyPem))
                {
                    return (false, "Morate prvo generisati ključeve za password manager", null);
                }

                // Kreiranje PasswordEntry
                var entry = new PasswordEntry
                {
                    SiteName = dto.SiteName,
                    Username = dto.Username,
                    OwnerId = userId,
                    CreatedAt = DateTime.UtcNow
                };

                var createdEntry = await _passwordRepository.CreateAsync(entry);

                // Kreiranje PasswordShare za ownera
                var share = new PasswordShare
                {
                    PasswordEntryId = createdEntry.Id,
                    UserId = userId,
                    EncryptedPassword = dto.EncryptedPassword,
                    SharedAt = DateTime.UtcNow
                };

                // Dodavanje share-a kroz context (nije potreban poseban repository metod za ovo)
                createdEntry.Shares.Add(share);
                await _passwordRepository.UpdateAsync(createdEntry);

                _logger.LogInformation("Lozinka kreirana za korisnika {UserId}, entry {EntryId}", userId, createdEntry.Id);

                var entryDto = MapToDto(createdEntry, userId);
                entryDto.EncryptedPassword = dto.EncryptedPassword; // Dodaj enkriptovanu lozinku u response

                return (true, "Lozinka je uspešno sačuvana", entryDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška pri kreiranju lozinke za korisnika {UserId}", userId);
                return (false, "Greška pri čuvanju lozinke", null);
            }
        }

        public async Task<(bool success, string message)> UpdatePasswordAsync(int userId, UpdatePasswordDto dto)
        {
            try
            {
                var entry = await _passwordRepository.GetByIdAsync(dto.Id);

                if (entry == null)
                {
                    return (false, "Lozinka nije pronađena");
                }

                // Samo owner može ažurirati
                if (entry.OwnerId != userId)
                {
                    return (false, "Nemate dozvolu da ažurirate ovu lozinku");
                }

                // Ažuriranje polja
                if (!string.IsNullOrEmpty(dto.SiteName))
                {
                    entry.SiteName = dto.SiteName;
                }

                if (!string.IsNullOrEmpty(dto.Username))
                {
                    entry.Username = dto.Username;
                }

                if (!string.IsNullOrEmpty(dto.EncryptedPassword))
                {
                    // Ažuriranje enkriptovane lozinke u owner share
                    var ownerShare = await _passwordRepository.GetPasswordShareAsync(dto.Id, userId);
                    if (ownerShare != null)
                    {
                        ownerShare.EncryptedPassword = dto.EncryptedPassword;
                    }
                }

                await _passwordRepository.UpdateAsync(entry);

                _logger.LogInformation("Lozinka {EntryId} ažurirana od strane korisnika {UserId}", dto.Id, userId);

                return (true, "Lozinka je uspešno ažurirana");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška pri ažuriranju lozinke {Id}", dto.Id);
                return (false, "Greška pri ažuriranju lozinke");
            }
        }

        public async Task<(bool success, string message)> DeletePasswordAsync(int id, int userId)
        {
            try
            {
                var entry = await _passwordRepository.GetByIdAsync(id);

                if (entry == null)
                {
                    return (false, "Lozinka nije pronađena");
                }

                // Samo owner može obrisati
                if (entry.OwnerId != userId)
                {
                    return (false, "Nemate dozvolu da obrišete ovu lozinku");
                }

                await _passwordRepository.DeleteAsync(id);

                _logger.LogInformation("Lozinka {EntryId} obrisana od strane korisnika {UserId}", id, userId);

                return (true, "Lozinka je uspešno obrisana");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška pri brisanju lozinke {Id}", id);
                return (false, "Greška pri brisanju lozinke");
            }
        }

        private PasswordEntryDto MapToDto(PasswordEntry entry, int currentUserId)
        {
            var share = entry.Shares.FirstOrDefault(s => s.UserId == currentUserId);

            return new PasswordEntryDto
            {
                Id = entry.Id,
                SiteName = entry.SiteName,
                Username = entry.Username,
                EncryptedPassword = share?.EncryptedPassword ?? string.Empty,
                IsOwner = entry.OwnerId == currentUserId,
                OwnerEmail = entry.Owner.Email,
                CreatedAt = entry.CreatedAt,
                UpdatedAt = entry.UpdatedAt
            };
        }
    }
}
