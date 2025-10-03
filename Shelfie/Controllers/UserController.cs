
using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shelfie.Services;

namespace Shelfie.Controllers;

[ApiController]
[Route("[controller]")]
public partial class UserController(IUserService userService) : ControllerBase
{
    public record GoogleLoginRequest(string AuthCode);
    public record RegisterRequest(string Username);
    
    [GeneratedRegex("^[a-zA-Z0-9]+$")]
    private static partial Regex UsernameRegex();
    
    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult> GetUserInfo()
    {
        var email = User.FindFirst(c => c.Type == System.Security.Claims.ClaimTypes.Email)!.Value;

        var user = await userService.GetUser(email);
        if (user == null || string.IsNullOrEmpty(user.UserName))
            return NotFound();

        return Ok(new { username = user.UserName, email });
    }
    
    [HttpPost("googleLogin")]
    public async Task<ActionResult> GoogleLogin([FromBody] GoogleLoginRequest request)
    {
        var jwt = await userService.GoogleLogin(request.AuthCode);
        
        if (jwt == null) return BadRequest("Failed to exchange auth code.");
        
        return Ok(jwt);
    }
    
    [Authorize]
    [HttpPost("register")]
    public async Task<ActionResult> Register([FromBody] RegisterRequest request)
    {
        var email = User.FindFirst(c => c.Type == System.Security.Claims.ClaimTypes.Email)?.Value;
        var username = request.Username.Trim();
        
        if (string.IsNullOrWhiteSpace(username))
            return BadRequest("Username cannot be empty.");
        
        if (username.Length is < 3 or > 20)
            return BadRequest("Username must be between 3 and 20 characters.");

        if (!UsernameRegex().IsMatch(username))
            return BadRequest("Username can only contain letters and numbers.");
        
        var success = await userService.Register(email, username);
        
        if (!success) return BadRequest();
        
        return Ok(email);
    }
}