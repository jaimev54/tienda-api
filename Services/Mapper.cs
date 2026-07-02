using TiendaApi.DTOs;
using TiendaApi.Models;

namespace TiendaApi.Services;

public static class Mapper
{
    public static UserDto ToDto(User u) => new(
    u.Id, u.Name, u.Email, u.Role, u.CreatedAt,
    u.Phone, u.Street, u.Colony,
    u.City, u.State, u.ZipCode, u.Country
    );
    public static CategoryDto ToDto(Category c) => new(c.Id, c.Name, c.Description);
    public static ProductDto ToDto(Product p) => new(p.Id, p.Name, p.Description, p.Price, p.Stock, p.ImageUrl, p.CategoryId, p.Category != null ? ToDto(p.Category) : null, p.CreatedAt);
    public static CartItemDto ToDto(CartItem ci) => new(ci.Id, ci.ProductId, ToDto(ci.Product!), ci.Quantity);
    public static CartDto ToDto(Cart cart)
    {
        var items = cart.Items.Select(ToDto).ToList();
        return new CartDto(items, items.Sum(i => i.Product.Price * i.Quantity), items.Sum(i => i.Quantity));
    }
    public static OrderItemDto ToDto(OrderItem oi) => new(oi.Id, oi.ProductId, ToDto(oi.Product!), oi.Quantity, oi.Price);
    public static OrderDto ToDto(Order o) => new(o.Id, o.UserId, o.User != null ? ToDto(o.User) : null, o.Items.Select(ToDto).ToList(), o.Total, o.Status, o.ShippingAddress, o.CreatedAt, o.UpdatedAt);
}
