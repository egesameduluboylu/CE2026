using BuildingBlocks.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;

namespace Host.Api.Controllers.Admin;

[ApiController]
[Route("api/admin")]
[Authorize(Policy = "admin")]
public class AdminVersionController : ControllerBase
{
    /// <summary>GET /api/admin/version</summary>
    [HttpGet("version")]
    public IActionResult Version()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var version = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
            ?? assembly.GetName().Version?.ToString() ?? "1.0.0";
        var data = new { version, environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production" };
        return Ok(ApiResponse.Ok(data, HttpContext.TraceIdentifier));
    }
}
