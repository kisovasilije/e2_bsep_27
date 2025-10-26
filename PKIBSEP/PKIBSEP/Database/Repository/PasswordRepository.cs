using Microsoft.EntityFrameworkCore;
using PKIBSEP.Interfaces.Repository;
using PKIBSEP.Models;

namespace PKIBSEP.Database.Repository
{
    public class PasswordRepository : IPasswordRepository
    {
        private readonly ApplicationDbContext _context;

        public PasswordRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<PasswordEntry>> GetUserPasswordsAsync(int userId)
        {
            // Vraća sve password entries gde je korisnik owner ili ima share
            var ownedPasswords = await _context.PasswordEntries
                .Include(pe => pe.Owner)
                .Include(pe => pe.Shares)
                .Where(pe => pe.OwnerId == userId)
                .ToListAsync();

            var sharedPasswords = await _context.PasswordShares
                .Include(ps => ps.PasswordEntry)
                    .ThenInclude(pe => pe.Owner)
                .Where(ps => ps.UserId == userId && ps.PasswordEntry.OwnerId != userId)
                .Select(ps => ps.PasswordEntry)
                .ToListAsync();

            // Kombinuj owned i shared passwords
            var allPasswords = ownedPasswords.Concat(sharedPasswords).ToList();

            return allPasswords;
        }

        public async Task<PasswordEntry?> GetByIdAsync(int id)
        {
            return await _context.PasswordEntries
                .Include(pe => pe.Owner)
                .Include(pe => pe.Shares)
                .FirstOrDefaultAsync(pe => pe.Id == id);
        }

        public async Task<PasswordEntry> CreateAsync(PasswordEntry entry)
        {
            _context.PasswordEntries.Add(entry);
            await _context.SaveChangesAsync();
            return entry;
        }

        public async Task UpdateAsync(PasswordEntry entry)
        {
            entry.UpdatedAt = DateTime.UtcNow;
            _context.PasswordEntries.Update(entry);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var entry = await _context.PasswordEntries.FindAsync(id);
            if (entry != null)
            {
                _context.PasswordEntries.Remove(entry);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<PasswordShare?> GetPasswordShareAsync(int entryId, int userId)
        {
            return await _context.PasswordShares
                .FirstOrDefaultAsync(ps => ps.PasswordEntryId == entryId && ps.UserId == userId);
        }
    }
}
