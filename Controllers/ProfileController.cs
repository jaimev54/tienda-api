using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TiendaApi.Data;
using TiendaApi.DTOs;
using TiendaApi.Services;

namespace TiendaApi.Controllers;

[ApiController]
[Route("api/profile")]
[Authorize]
public class ProfileController : ControllerBase
{
    private readonly AppDbContext _db;
    public ProfileController(AppDbContext db) => _db = db;

    private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<ActionResult<UserDto>> GetProfile()
    {
        var user = await _db.Users.FindAsync(UserId);
        if (user == null) return NotFound();
        return Ok(Mapper.ToDto(user));
    }

    [HttpPut]
    public async Task<ActionResult<UserDto>> UpdateProfile(UpdateProfileRequest req)
    {
        var user = await _db.Users.FindAsync(UserId);
        if (user == null) return NotFound();

        if (req.Name != null) user.Name = req.Name;
        if (req.Phone != null) user.Phone = req.Phone;
        if (req.Street != null) user.Street = req.Street;
        if (req.Colony != null) user.Colony = req.Colony;
        if (req.City != null) user.City = req.City;
        if (req.State != null) user.State = req.State;
        if (req.ZipCode != null) user.ZipCode = req.ZipCode;
        if (req.Country != null) user.Country = req.Country;

        await _db.SaveChangesAsync();
        return Ok(Mapper.ToDto(user));
    }
}