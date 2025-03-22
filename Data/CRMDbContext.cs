using System.Security.Cryptography;
using System.Text;
using jekirdekCase.Models;
using Microsoft.EntityFrameworkCore;

namespace jekirdekCase.Data
{
    public class CRMDbContext : DbContext
    {
        public CRMDbContext(DbContextOptions<CRMDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Customer> Customers { get; set; } // ✅ HATA DÜZELTİLDİ!

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 📌 Varsayılan kullanıcıları ekleyelim
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    Username = "admin",
                    Password = HashPassword("admin123"), // Hashlenmiş şifre
                    Role = "Admin",
                    CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) // ✅ SABİT TARİH
                },
                new User
                {
                    Id = 2,
                    Username = "user",
                    Password = HashPassword("user123"), // Hashlenmiş şifre
                    Role = "User",
                    CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) // ✅ SABİT TARİH
                }
            );
        }

        // 📌 Şifre Hashleme Fonksiyonu (SHA-256)
        private static string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }
    }
}