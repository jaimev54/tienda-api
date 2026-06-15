using Microsoft.EntityFrameworkCore;
using TiendaApi.Models;

namespace TiendaApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Cart> Carts => Set<Cart>();
    public DbSet<CartItem> CartItems => Set<CartItem>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();
        modelBuilder.Entity<Cart>().HasIndex(c => c.UserId).IsUnique();

        modelBuilder.Entity<CartItem>()
            .HasOne(ci => ci.Cart).WithMany(c => c.Items)
            .HasForeignKey(ci => ci.CartId).OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<OrderItem>()
            .HasOne(oi => oi.Order).WithMany(o => o.Items)
            .HasForeignKey(oi => oi.OrderId).OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Category>().HasData(
            new Category { Id = 1, Name = "Electronica", Description = "Dispositivos y gadgets" },
            new Category { Id = 2, Name = "Ropa", Description = "Moda y accesorios" },
            new Category { Id = 3, Name = "Hogar", Description = "Articulos para el hogar" },
            new Category { Id = 4, Name = "Deportes", Description = "Equipamiento deportivo" },
            new Category { Id = 5, Name = "Libros", Description = "Libros y material educativo" }
        );

        modelBuilder.Entity<User>().HasData(
            new User { Id = 1, Name = "Admin", Email = "admin@tienda.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"), Role = "admin", CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new User { Id = 2, Name = "Cliente Demo", Email = "cliente@tienda.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("Cliente123!"), Role = "customer", CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
        );

        modelBuilder.Entity<Product>().HasData(
            new Product { Id = 1, Name = "Laptop Pro 15", Description = "Laptop de alto rendimiento con procesador i9", Price = 25999.99m, Stock = 10, CategoryId = 1, ImageUrl = "https://images.unsplash.com/photo-1496181133206-80ce9b88a853?w=400", CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Product { Id = 2, Name = "Smartphone X12", Description = "Telefono con camara de 108MP y pantalla AMOLED", Price = 12499.99m, Stock = 25, CategoryId = 1, ImageUrl = "https://images.unsplash.com/photo-1511707171634-5f897ff02aa9?w=400", CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Product { Id = 3, Name = "Audifonos Wireless", Description = "Cancelacion de ruido activa, 30h de bateria", Price = 2999.99m, Stock = 50, CategoryId = 1, ImageUrl = "https://images.unsplash.com/photo-1505740420928-5e560c06d30e?w=400", CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Product { Id = 4, Name = "Playera Premium", Description = "100% algodon organico, corte slim", Price = 349.99m, Stock = 100, CategoryId = 2, ImageUrl = "https://images.unsplash.com/photo-1521572163474-6864f9cf17ab?w=400", CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Product { Id = 5, Name = "Tenis Running", Description = "Amortiguacion avanzada para largas distancias", Price = 1899.99m, Stock = 30, CategoryId = 4, ImageUrl = "https://images.unsplash.com/photo-1542291026-7eec264c27ff?w=400", CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Product { Id = 6, Name = "Silla Ergonomica", Description = "Soporte lumbar ajustable, ideal para home office", Price = 4599.99m, Stock = 15, CategoryId = 3, ImageUrl = "https://images.unsplash.com/photo-1592078615290-033ee584e267?w=400", CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Product { Id = 7, Name = "Clean Code", Description = "Robert C. Martin, guia de buenas practicas", Price = 499.99m, Stock = 40, CategoryId = 5, ImageUrl = "https://images.unsplash.com/photo-1589998059171-988d887df646?w=400", CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Product { Id = 8, Name = "Smartwatch Serie 5", Description = "Monitor cardiaco, GPS, resistente al agua", Price = 3799.99m, Stock = 20, CategoryId = 1, ImageUrl = "https://images.unsplash.com/photo-1523275335684-37898b6baf30?w=400", CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
        );
    }
}
