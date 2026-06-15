using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TiendaApi.Data;
using TiendaApi.DTOs;
using TiendaApi.Services;

namespace TiendaApi.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "admin")]
public class AdminController : ControllerBase
{
    private readonly AppDbContext _db;
    public AdminController(AppDbContext db) => _db = db;

    [HttpGet("stats")]
    public async Task<ActionResult<DashboardStats>> GetStats()
    {
        var totalRevenue = await _db.Orders.Where(o => o.Status != "cancelled").SumAsync(o => o.Total);
        var totalOrders = await _db.Orders.CountAsync();
        var totalProducts = await _db.Products.CountAsync();
        var totalUsers = await _db.Users.CountAsync();

        var recentOrders = await _db.Orders
            .Include(o => o.User)
            .Include(o => o.Items).ThenInclude(i => i.Product).ThenInclude(p => p!.Category)
            .OrderByDescending(o => o.CreatedAt)
            .Take(5)
            .ToListAsync();

        var topProducts = await _db.OrderItems
            .Include(oi => oi.Product).ThenInclude(p => p!.Category)
            .GroupBy(oi => oi.ProductId)
            .Select(g => new { ProductId = g.Key, Sold = g.Sum(oi => oi.Quantity), Product = g.First().Product })
            .OrderByDescending(x => x.Sold)
            .Take(5)
            .ToListAsync();

        return Ok(new DashboardStats(
            totalRevenue, totalOrders, totalProducts, totalUsers,
            recentOrders.Select(Mapper.ToDto).ToList(),
            topProducts.Select(x => new TopProductDto(Mapper.ToDto(x.Product!), x.Sold)).ToList()
        ));
    }
}
