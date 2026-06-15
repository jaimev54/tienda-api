using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TiendaApi.Data;
using TiendaApi.DTOs;
using TiendaApi.Services;

namespace TiendaApi.Controllers;

[ApiController]
[Route("api/categories")]
public class CategoriesController : ControllerBase
{
    private readonly AppDbContext _db;
    public CategoriesController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<List<CategoryDto>>> GetCategories()
    {
        var cats = await _db.Categories.ToListAsync();
        return Ok(cats.Select(Mapper.ToDto).ToList());
    }
}
