using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Modules.Identity.Infrastructure.Persistence;
using Modules.Identity.Infrastructure.Persistence.Entities;
using Modules.Identity.Infrastructure.Services;

namespace Host.Api.Controllers;

[ApiController]
[Route("api/debug/auth")]
public class AuthDebugController : ControllerBase
{
    private readonly AuthDbContext _db;
    private readonly PasswordService _pw;

    public AuthDebugController(AuthDbContext db, PasswordService pw)
    {
        _db = db;
        _pw = pw;
    }

    [HttpPost("create-user")]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest req)
    {
        var email = req.Email.Trim().ToLowerInvariant();
        if (await _db.Users.AnyAsync(u => u.Email == email))
            return BadRequest(new { error = "Email already exists." });

        var user = new AppUser
        {
            Email = email,
            UserName = email,
            PasswordHash = _pw.Hash(req.Password)
        };
        _db.Users.Add(user);
        await _db.SaveChangesAsync();
        return Ok(new { user.Id, user.Email });
    }
}

public record CreateUserRequest(string Email, string Password);