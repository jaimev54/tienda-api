using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TiendaApi.Data;
using TiendaApi.DTOs;
using TiendaApi.Models;
using TiendaApi.Services;

namespace TiendaApi.Controllers;

[ApiController]
[Route("api/orders")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly AppDbContext _db;
    public OrdersController(AppDbContext db) => _db = db;

    private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet("my")]
    public async Task<ActionResult<List<OrderDto>>> GetMyOrders()
    {
        var orders = await _db.Orders
            .Include(o => o.User)
            .Include(o => o.Items).ThenInclude(i => i.Product).ThenInclude(p => p!.Category)
            .Where(o => o.UserId == UserId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
        return Ok(orders.Select(Mapper.ToDto).ToList());
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<OrderDto>> GetOrder(int id)
    {
        var order = await _db.Orders
            .Include(o => o.User)
            .Include(o => o.Items).ThenInclude(i => i.Product).ThenInclude(p => p!.Category)
            .FirstOrDefaultAsync(o => o.Id == id && o.UserId == UserId);
        if (order == null) return NotFound();
        return Ok(Mapper.ToDto(order));
    }

    [HttpPost]
    public async Task<ActionResult<OrderDto>> CreateOrder(CreateOrderRequest req)
    {
        var cart = await _db.Carts
            .Include(c => c.Items).ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(c => c.UserId == UserId);

        if (cart == null || !cart.Items.Any())
            return BadRequest(new { message = "El carrito esta vacio" });

        var order = new Order
        {
            UserId = UserId,
            ShippingAddress = req.ShippingAddress,
            Status = "pending",
            Total = cart.Items.Sum(i => i.Product!.Price * i.Quantity),
            Items = cart.Items.Select(i => new OrderItem
            {
                ProductId = i.ProductId,
                Quantity = i.Quantity,
                Price = i.Product!.Price
            }).ToList()
        };

        foreach (var item in cart.Items)
        {
            var product = await _db.Products.FindAsync(item.ProductId);
            if (product != null) product.Stock -= item.Quantity;
        }

        _db.Orders.Add(order);
        _db.CartItems.RemoveRange(cart.Items);
        await _db.SaveChangesAsync();

        await _db.Entry(order).Reference(o => o.User).LoadAsync();
        foreach (var item in order.Items)
            await _db.Entry(item).Reference(i => i.Product).Query().Include(p => p.Category).LoadAsync();

        return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, Mapper.ToDto(order));
    }

    [HttpGet, Authorize(Roles = "admin")]
    public async Task<ActionResult<PaginatedResult<OrderDto>>> GetAllOrders([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var query = _db.Orders
            .Include(o => o.User)
            .Include(o => o.Items).ThenInclude(i => i.Product).ThenInclude(p => p!.Category)
            .OrderByDescending(o => o.CreatedAt);

        var total = await query.CountAsync();
        var data = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        return Ok(new PaginatedResult<OrderDto>(data.Select(Mapper.ToDto).ToList(), total, page, pageSize, (int)Math.Ceiling((double)total / pageSize)));
    }

    [HttpPut("{id}/status"), Authorize(Roles = "admin")]
    public async Task<ActionResult<OrderDto>> UpdateStatus(int id, UpdateOrderStatusRequest req)
    {
        var order = await _db.Orders
            .Include(o => o.User)
            .Include(o => o.Items).ThenInclude(i => i.Product).ThenInclude(p => p!.Category)
            .FirstOrDefaultAsync(o => o.Id == id);
        if (order == null) return NotFound();
        order.Status = req.Status;
        order.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return Ok(Mapper.ToDto(order));
    }
}
