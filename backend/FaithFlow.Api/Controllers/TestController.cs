using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FaithFlow.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TestController : ControllerBase
{
    [HttpGet("public")]
    public IActionResult Public()
    {
        return Ok(new { message = "This is a public endpoint - anyone can access" });
    }

    [Authorize]
    [HttpGet("protected")]
    public IActionResult Protected()
    {
        return Ok(new { 
            message = "This is protected - you are authenticated!",
            user = User.Identity?.Name ?? "Unknown"
        });
    }
}