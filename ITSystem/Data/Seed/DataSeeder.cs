using ITSystem.Data.Contexts;
using ITSystem.Data.Entities;
using BCrypt.Net;
using Microsoft.EntityFrameworkCore;

namespace ITSystem.Data
{
    public static class DataSeeder
    {
        public static void SeedIfEmpty(OrderDbContext dbContext)
        {
            if (!dbContext.Products.Any())
            {
                dbContext.Products.AddRange(
                    new Product { Name = "Produkt 1", Description = "Beskrivning 1", Price = 100 },
                    new Product { Name = "Produkt 2", Description = "Beskrivning 2", Price = 200 }
                );
            }

            if (!dbContext.Users.Any())
            {
                var hashedPassword = BCrypt.Net.BCrypt.HashPassword("admin123");
                dbContext.Users.Add(new User
                {
                    Username = "admin",
                    PasswordHash = hashedPassword,
                    Role = "Admin"
                });
            }

            dbContext.SaveChanges();
        }
    }
}
