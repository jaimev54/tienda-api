using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TiendaApi.Data;
using TiendaApi.DTOs;
using TiendaApi.Models;
using TiendaApi.Services;

namespace TiendaApi.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly TokenService _tokenService;

    public AuthController(AppDbContext db, TokenService tokenService)
    { _db = db; _tokenService = tokenService; }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest req)
    {
        if (await _db.Users.AnyAsync(u => u.Email == req.Email.ToLower()))
            return BadRequest(new { message = "El correo ya esta registrado" });

        var user = new User
        {
            Name = req.Name,
            Email = req.Email.ToLower(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password),
            Role = "customer"
        };
        _db.Users.Add(user);
        await _db.SaveChangesAsync();
        _db.Carts.Add(new Cart { UserId = user.Id });
        await _db.SaveChangesAsync();
        return Ok(new AuthResponse(_tokenService.GenerateToken(user), Mapper.ToDto(user)));
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest req)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == req.Email.ToLower());
        if (user == null || !BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash))
            return Unauthorized(new { message = "Credenciales invalidas" });
        return Ok(new AuthResponse(_tokenService.GenerateToken(user), Mapper.ToDto(user)));
    }
}
