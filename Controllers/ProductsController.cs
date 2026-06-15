using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TiendaApi.Data;
using TiendaApi.DTOs;
using TiendaApi.Models;
using TiendaApi.Services;

namespace TiendaApi.Controllers;

[ApiController]
[Route("api/products")]
public class ProductsController : ControllerBase
{
    private readonly AppDbContext _db;
    public ProductsController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<PaginatedResult<ProductDto>>> GetProducts([FromQuery] ProductFilter filter)
    {
        var query = _db.Products.Include(p => p.Category).AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Search))
            query = query.Where(p => p.Name.ToLower().Contains(filter.Search.ToLower()) ||
                                     p.Description.ToLower().Contains(filter.Search.ToLower()));

        if (filter.CategoryId.HasValue)
            query = query.Where(p => p.CategoryId == filter.CategoryId.Value);

        if (filter.MinPrice.HasValue)
            query = query.Where(p => p.Price >= filter.MinPrice.Value);

        if (filter.MaxPrice.HasValue)
            query = query.Where(p => p.Price <= filter.MaxPrice.Value);

        query = filter.SortBy switch
        {
            "price_asc" => query.OrderBy(p => p.Price),
            "price_desc" => query.OrderByDescending(p => p.Price),
            "name" => query.OrderBy(p => p.Name),
            _ => query.OrderByDescending(p => p.CreatedAt)
        };

        var total = await query.CountAsync();
        var page = filter.Page < 1 ? 1 : filter.Page;
        var pageSize = filter.PageSize < 1 ? 12 : filter.PageSize;
        var data = await query.Skip((page - 1) * pageSize).Take(pageSize).Select(p => Mapper.ToDto(p)).ToListAsync();

        return Ok(new PaginatedResult<ProductDto>(data, total, page, pageSize, (int)Math.Ceiling((double)total / pageSize)));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ProductDto>> GetProduct(int id)
    {
        var product = await _db.Products.Include(p => p.Category).FirstOrDefaultAsync(p => p.Id == id);
        if (product == null) return NotFound();
        return Ok(Mapper.ToDto(product));
    }

    [HttpPost, Authorize(Roles = "admin")]
    public async Task<ActionResult<ProductDto>> CreateProduct(CreateProductRequest req)
    {
        var product = new Product
        {
            Name = req.Name, Description = req.Description, Price = req.Price,
            Stock = req.Stock, ImageUrl = req.ImageUrl, CategoryId = req.CategoryId
        };
        _db.Products.Add(product);
        await _db.SaveChangesAsync();
        await _db.Entry(product).Reference(p => p.Category).LoadAsync();
        return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, Mapper.ToDto(product));
    }

    [HttpPut("{id}"), Authorize(Roles = "admin")]
    public async Task<ActionResult<ProductDto>> UpdateProduct(int id, UpdateProductRequest req)
    {
        var product = await _db.Products.Include(p => p.Category).FirstOrDefaultAsync(p => p.Id == id);
        if (product == null) return NotFound();
        if (req.Name != null) product.Name = req.Name;
        if (req.Description != null) product.Description = req.Description;
        if (req.Price.HasValue) product.Price = req.Price.Value;
        if (req.Stock.HasValue) product.Stock = req.Stock.Value;
        if (req.ImageUrl != null) product.ImageUrl = req.ImageUrl;
        if (req.CategoryId.HasValue) product.CategoryId = req.CategoryId.Value;
        await _db.SaveChangesAsync();
        await _db.Entry(product).Reference(p => p.Category).LoadAsync();
        return Ok(Mapper.ToDto(product));
    }

    [HttpDelete("{id}"), Authorize(Roles = "admin")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        var product = await _db.Products.FindAsync(id);
        if (product == null) return NotFound();
        _db.Products.Remove(product);
        await _db.SaveChangesAsync();
        return Ok(new { success = true });
    }
}
