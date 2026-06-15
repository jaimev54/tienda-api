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
[Route("api/cart")]
[Authorize]
public class CartController : ControllerBase
{
    private readonly AppDbContext _db;
    public CartController(AppDbContext db) => _db = db;

    private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    private async Task<Cart> GetOrCreateCart()
    {
        var cart = await _db.Carts.Include(c => c.Items).ThenInclude(i => i.Product).ThenInclude(p => p!.Category)
            .FirstOrDefaultAsync(c => c.UserId == UserId);
        if (cart == null)
        {
            cart = new Cart { UserId = UserId };
            _db.Carts.Add(cart);
            await _db.SaveChangesAsync();
        }
        return cart;
    }

    [HttpGet]
    public async Task<ActionResult<CartDto>> GetCart()
    {
        var cart = await GetOrCreateCart();
        return Ok(Mapper.ToDto(cart));
    }

    [HttpPost("items")]
    public async Task<ActionResult<CartDto>> AddItem(AddToCartRequest req)
    {
        var cart = await GetOrCreateCart();
        var product = await _db.Products.FindAsync(req.ProductId);
        if (product == null) return NotFound(new { message = "Producto no encontrado" });
        if (product.Stock < req.Quantity) return BadRequest(new { message = "Stock insuficiente" });

        var existing = cart.Items.FirstOrDefault(i => i.ProductId == req.ProductId);
        if (existing != null) existing.Quantity += req.Quantity;
        else cart.Items.Add(new CartItem { CartId = cart.Id, ProductId = req.ProductId, Quantity = req.Quantity });

        await _db.SaveChangesAsync();
        await _db.Entry(cart).Collection(c => c.Items).Query()
            .Include(i => i.Product).ThenInclude(p => p!.Category).LoadAsync();
        return Ok(Mapper.ToDto(cart));
    }

    [HttpPut("items/{itemId}")]
    public async Task<ActionResult<CartDto>> UpdateItem(int itemId, UpdateCartItemRequest req)
    {
        var cart = await GetOrCreateCart();
        var item = cart.Items.FirstOrDefault(i => i.Id == itemId);
        if (item == null) return NotFound();
        item.Quantity = req.Quantity;
        await _db.SaveChangesAsync();
        return Ok(Mapper.ToDto(cart));
    }

    [HttpDelete("items/{itemId}")]
    public async Task<ActionResult<CartDto>> RemoveItem(int itemId)
    {
        var cart = await GetOrCreateCart();
        var item = cart.Items.FirstOrDefault(i => i.Id == itemId);
        if (item == null) return NotFound();
        cart.Items.Remove(item);
        _db.CartItems.Remove(item);
        await _db.SaveChangesAsync();
        return Ok(Mapper.ToDto(cart));
    }

    [HttpDelete]
    public async Task<IActionResult> ClearCart()
    {
        var cart = await GetOrCreateCart();
        _db.CartItems.RemoveRange(cart.Items);
        await _db.SaveChangesAsync();
        return Ok();
    }
}
