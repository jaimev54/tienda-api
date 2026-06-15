using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TiendaApi.Models;

public class User
{
    public int Id { get; set; }
    [Required, MaxLength(100)] public string Name { get; set; } = string.Empty;
    [Required, MaxLength(150)] public string Email { get; set; } = string.Empty;
    [Required] public string PasswordHash { get; set; } = string.Empty;
    [MaxLength(20)] public string Role { get; set; } = "customer";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public ICollection<Order> Orders { get; set; } = new List<Order>();
    public Cart? Cart { get; set; }
}

public class Category
{
    public int Id { get; set; }
    [Required, MaxLength(80)] public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ICollection<Product> Products { get; set; } = new List<Product>();
}

public class Product
{
    public int Id { get; set; }
    [Required, MaxLength(150)] public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    [Column(TypeName = "decimal(18,2)")] public decimal Price { get; set; }
    public int Stock { get; set; }
    public string? ImageUrl { get; set; }
    public int CategoryId { get; set; }
    public Category? Category { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}

public class Cart
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User? User { get; set; }
    public ICollection<CartItem> Items { get; set; } = new List<CartItem>();
}

public class CartItem
{
    public int Id { get; set; }
    public int CartId { get; set; }
    public Cart? Cart { get; set; }
    public int ProductId { get; set; }
    public Product? Product { get; set; }
    public int Quantity { get; set; }
}

public class Order
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User? User { get; set; }
    [Column(TypeName = "decimal(18,2)")] public decimal Total { get; set; }
    [MaxLength(30)] public string Status { get; set; } = "pending";
    public string ShippingAddress { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
}

public class OrderItem
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public Order? Order { get; set; }
    public int ProductId { get; set; }
    public Product? Product { get; set; }
    public int Quantity { get; set; }
    [Column(TypeName = "decimal(18,2)")] public decimal Price { get; set; }
}
