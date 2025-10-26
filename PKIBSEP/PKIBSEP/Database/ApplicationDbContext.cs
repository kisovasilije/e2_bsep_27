using Microsoft.EntityFrameworkCore;
using PKIBSEP.Models;

namespace PKIBSEP.Database
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }

        public DbSet<Session> Sessions { get; set; }

        public DbSet<PasswordEntry> PasswordEntries { get; set; }

        public DbSet<PasswordShare> PasswordShares { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User entity configuration
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.Role).HasConversion<string>();
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            });

            // Session entity configuration
            modelBuilder.Entity<Session>()
                .HasIndex(s => s.JwtHash)
                .IsUnique();

            // PasswordEntry entity configuration
            modelBuilder.Entity<PasswordEntry>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.HasOne(e => e.Owner)
                    .WithMany()
                    .HasForeignKey(e => e.OwnerId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // PasswordShare entity configuration
            modelBuilder.Entity<PasswordShare>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.SharedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.HasOne(e => e.PasswordEntry)
                    .WithMany(pe => pe.Shares)
                    .HasForeignKey(e => e.PasswordEntryId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
