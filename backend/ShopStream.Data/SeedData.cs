using Microsoft.EntityFrameworkCore;
using ShopStream.Core.Entities;

namespace ShopStream.Data;

public static class SeedData
{
    public static async Task Initialize(ApplicationDbContext context)
    {
        await context.Database.MigrateAsync();

        // Check if data already exists
        if (await context.Users.AnyAsync())
        {
            return; // DB has been seeded
        }

        // Seed Categories
        var electronics = new Category { Id = Guid.NewGuid(), Name = "Electronics", Description = "Electronic devices and accessories" };
        var clothing = new Category { Id = Guid.NewGuid(), Name = "Clothing", Description = "Apparel and fashion" };
        var books = new Category { Id = Guid.NewGuid(), Name = "Books", Description = "Books and literature" };
        
        await context.Categories.AddRangeAsync(electronics, clothing, books);
        await context.SaveChangesAsync();

        // Seed Users (passwords are hashed with BCrypt)
        // Password for all: "Password123!"
        var adminUser = new User
        {
            Id = Guid.NewGuid(),
            Email = "admin@shopstream.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!"),
            FirstName = "Admin",
            LastName = "User",
            Role = UserRole.Admin,
            IsEmailConfirmed = true
        };

        var customerUser = new User
        {
            Id = Guid.NewGuid(),
            Email = "customer@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!"),
            FirstName = "John",
            LastName = "Doe",
            Role = UserRole.Customer,
            IsEmailConfirmed = true
        };

        await context.Users.AddRangeAsync(adminUser, customerUser);
        await context.SaveChangesAsync();

        // Seed Products
        var products = new List<Product>
        {
            new Product
            {
                Id = Guid.NewGuid(),
                Name = "Laptop Pro 15",
                SKU = "ELEC-LAP-001",
                Description = "High-performance laptop with 16GB RAM and 512GB SSD",
                Price = 1299.99m,
                StockQuantity = 50,
                CategoryId = electronics.Id,
                IsActive = true
            },
            new Product
            {
                Id = Guid.NewGuid(),
                Name = "Wireless Mouse",
                SKU = "ELEC-MOU-001",
                Description = "Ergonomic wireless mouse with precision tracking",
                Price = 29.99m,
                StockQuantity = 200,
                CategoryId = electronics.Id,
                IsActive = true
            },
            new Product
            {
                Id = Guid.NewGuid(),
                Name = "Cotton T-Shirt",
                SKU = "CLO-TSH-001",
                Description = "Comfortable 100% cotton t-shirt",
                Price = 19.99m,
                StockQuantity = 150,
                CategoryId = clothing.Id,
                IsActive = true
            },
            new Product
            {
                Id = Guid.NewGuid(),
                Name = "Programming Guide",
                SKU = "BOOK-PRG-001",
                Description = "Comprehensive guide to modern programming",
                Price = 49.99m,
                StockQuantity = 75,
                CategoryId = books.Id,
                IsActive = true
            }
        };

        await context.Products.AddRangeAsync(products);
        await context.SaveChangesAsync();

        // Seed Product Images (using placeholder.com for demo)
        foreach (var product in products)
        {
            await context.ProductImages.AddAsync(new ProductImage
            {
                Id = Guid.NewGuid(),
                ProductId = product.Id,
                Url = $"https://via.placeholder.com/400x400/FF9900/FFFFFF?text={product.Name.Replace(" ", "+")}",
                AltText = product.Name,
                DisplayOrder = 1
            });
        }

        await context.SaveChangesAsync();

        // Seed Address for customer
        var address = new Address
        {
            Id = Guid.NewGuid(),
            UserId = customerUser.Id,
            Street = "123 Main St",
            City = "New York",
            State = "NY",
            ZipCode = "10001",
            Country = "USA",
            IsDefault = true
        };

        await context.Addresses.AddAsync(address);
        await context.SaveChangesAsync();
    }
}
