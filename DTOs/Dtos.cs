using System.ComponentModel.DataAnnotations;

namespace TiendaApi.DTOs;

public record LoginRequest([Required, EmailAddress] string Email, [Required] string Password);
public record RegisterRequest([Required, MaxLength(100)] string Name, [Required, EmailAddress] string Email, [Required, MinLength(6)] string Password);
public record AuthResponse(string Token, UserDto User);
public record UserDto(int Id, string Name, string Email, string Role, DateTime CreatedAt);
public record CategoryDto(int Id, string Name, string? Description);
public record ProductDto(int Id, string Name, string Description, decimal Price, int Stock, string? ImageUrl, int CategoryId, CategoryDto? Category, DateTime CreatedAt);
public record CreateProductRequest([Required, MaxLength(150)] string Name, [Required] string Description, [Range(0, double.MaxValue)] decimal Price, [Range(0, int.MaxValue)] int Stock, string? ImageUrl, [Required] int CategoryId);
public record UpdateProductRequest(string? Name, string? Description, decimal? Price, int? Stock, string? ImageUrl, int? CategoryId);
public record ProductFilter(string? Search, int? CategoryId, decimal? MinPrice, decimal? MaxPrice, string? SortBy, int Page = 1, int PageSize = 12);
public record PaginatedResult<T>(List<T> Data, int Total, int Page, int PageSize, int TotalPages);
public record CartDto(List<CartItemDto> Items, decimal Total, int ItemCount);
public record CartItemDto(int Id, int ProductId, ProductDto Product, int Quantity);
public record AddToCartRequest([Required] int ProductId, [Range(1, 99)] int Quantity = 1);
public record UpdateCartItemRequest([Range(1, 99)] int Quantity);
public record OrderDto(int Id, int UserId, UserDto? User, List<OrderItemDto> Items, decimal Total, string Status, string ShippingAddress, DateTime CreatedAt, DateTime UpdatedAt);
public record OrderItemDto(int Id, int ProductId, ProductDto Product, int Quantity, decimal Price);
public record CreateOrderRequest([Required] string ShippingAddress);
public record UpdateOrderStatusRequest([Required] string Status);
public record DashboardStats(decimal TotalRevenue, int TotalOrders, int TotalProducts, int TotalUsers, List<OrderDto> RecentOrders, List<TopProductDto> TopProducts);
public record TopProductDto(ProductDto Product, int Sold);
