
using Microsoft.AspNetCore.Mvc;
using Shelfie.Services;

namespace Shelfie.Controllers;

[ApiController]
[Route("[controller]")]
public class UserController(IUserService userService) : ControllerBase
{
    public record GoogleLoginRequest(string AuthCode);
    
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
    public async Task<ActionResult<string>> GoogleLogin([FromBody] GoogleLoginRequest request)
    {
        var jwt = await userService.GoogleLogin(request.AuthCode);
        
        if (jwt == null) return BadRequest();
        
        return Ok(jwt);
    }
}