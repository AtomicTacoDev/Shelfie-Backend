
using Microsoft.AspNetCore.Mvc;
using Shelfie.Services;

namespace Shelfie.Controllers;

[ApiController]
[Route("[controller]")]
public class UserController(IUserService userService) : ControllerBase
{
    [HttpGet("validateToken")]
    public IActionResult ValidateToken()
    {
        var authHeader = HttpContext.Request.Headers["Authorization"].ToString();
        if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
            return BadRequest();
        
        var token = authHeader.Substring("Bearer ".Length).Trim();
        
        if (userService.ValidateToken(token)) return Ok();

        return Unauthorized();
    }
    
    [HttpPost("googleLogin")]
    public async Task<ActionResult<string>> GoogleLogin([FromBody] string authCode)
    {
        var jwt = await userService.GoogleLogin(authCode);
        
        if (jwt == null) return BadRequest();
        
        return Ok(jwt);
    }
}