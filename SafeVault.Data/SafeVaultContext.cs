using Microsoft.EntityFrameworkCore;
using SafeVault.Data.Entities;

namespace SafeVault.Data
{
    public class SafeVaultContext : DbContext
    {
        public SafeVaultContext(DbContextOptions<SafeVaultContext> options)
            : base(options) { }

        public DbSet<User> Users => Set<User>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(u => u.UserID);
                entity.Property(u => u.Username)
                      .HasMaxLength(100)
                      .IsRequired();
                entity.Property(u => u.Email)
                      .HasMaxLength(100)
                      .IsRequired();
                entity.Property(u => u.PasswordHash)
                      .IsRequired();
                entity.Property(u => u.Role)
                      .HasMaxLength(50)
                      .IsRequired();
                entity.HasIndex(u => u.Username).IsUnique();
                entity.HasIndex(u => u.Email).IsUnique();
            });
        }
    }
}

