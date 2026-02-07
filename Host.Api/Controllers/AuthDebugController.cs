using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;

namespace Host.Api.Controllers;

[ApiController]
[Route("api/debug/auth")]
public class AuthDebugController : ControllerBase
{
    private readonly UserManager<IdentityUser> _users;

    public AuthDebugController(UserManager<IdentityUser> users)
    {
        _users = users;
    }

    [HttpPost("create-user")]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest req)
    {
        var user = new IdentityUser { UserName = req.Email, Email = req.Email };
        var result = await _users.CreateAsync(user, req.Password);
        if (!result.Succeeded)
            return BadRequest(result.Errors);

        return Ok(new { user.Id, user.Email });
    }
}

public record CreateUserRequest(string Email, string Password);