using Microsoft.EntityFrameworkCore;
using Pki.Domain;
using PKIBSEP.Models;
using PKIBSEP.Models.Certificate;

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
        public DbSet<Certificate> Certificates { get; set; }
        public DbSet<CaKeyMaterial> CaKeyMaterials {  get; set; }
        public DbSet<CaUserKey> CaUserKeys { get; set; }
        public DbSet<CaAssignment> CaAssignments {  get; set; }

        public DbSet<PasswordEntry> PasswordEntries { get; set; }

        public DbSet<PasswordShare> PasswordShares { get; set; }

        public DbSet<Certificate2> Certificate2s { get; set; }

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

            //Certficate
            modelBuilder.Entity<Certificate>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.SerialHex).IsRequired().HasMaxLength(128);
                e.Property(x => x.SubjectDistinguishedName).IsRequired();
                e.Property(x => x.IssuerDistinguishedName).IsRequired();
                e.Property(x => x.PemCertificate).IsRequired();
                e.Property(x => x.ChainPem).IsRequired();
                e.Property(x => x.Organization).HasMaxLength(256);

                //Indexes
                e.HasIndex(x => x.SerialHex).IsUnique();
                e.HasIndex(x => x.ChainRootId);
                e.HasIndex(x => x.ParentCertificateId);
                e.HasIndex(x => x.Type);
            });

            //CaKeyMateral
            modelBuilder.Entity<CaKeyMaterial>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.PfxFilePath).IsRequired();
                e.Property(x => x.EncryptedPfxPassword).IsRequired();
                e.Property(x => x.KeystoreAlias).HasMaxLength(256);

                e.HasIndex(x => x.CertificateId).IsUnique();
            });

            // CaUserKey
            modelBuilder.Entity<CaUserKey>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.ProtectedWrapKey).IsRequired();
                e.Property(x => x.CreatedAtUtc).HasDefaultValueSql("CURRENT_TIMESTAMP");
                e.HasIndex(x => new { x.CaUserId, x.KeyVersion }).IsUnique();
            });

            // CaAssignment (optional)
            modelBuilder.Entity<CaAssignment>(e =>
            {
                e.HasKey(x => x.Id);
                e.HasIndex(x => new { x.CaUserId, x.ChainRootCertificateId }).IsUnique();
            });

            modelBuilder.Entity<Certificate2>(entity =>
            {
                entity.HasOne(c => c.Subject)
                    .WithMany()
                    .HasForeignKey(c => c.SubjectId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(c => c.Issuer)
                    .WithMany()
                    .HasForeignKey(c => c.IssuerId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
