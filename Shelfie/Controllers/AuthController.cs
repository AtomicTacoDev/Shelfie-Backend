
using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shelfie.Models.Dto;
using Shelfie.Services;

namespace Shelfie.Controllers;

[ApiController]
[Route("[controller]")]
public partial class AuthController(IAuthService authService) : ControllerBase
{
    public record GoogleLoginRequest(string AuthCode);
    public record RegisterRequest(string Username);
    public record SignUpRequest(string Email, string Password);
    
    [GeneratedRegex("^[a-zA-Z0-9]+$")]
    private static partial Regex UsernameRegex();
    
    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<UserDto>> Me()
    {
        var email = User.FindFirst(c => c.Type == System.Security.Claims.ClaimTypes.Email)!.Value;

        var user = await authService.GetUserByEmail(email);
        if (user == null || string.IsNullOrEmpty(user.UserName))
            return NotFound();

        return Ok(user);
    }
    
    [HttpPost("refresh")]
    public async Task<ActionResult<AuthResponseDto>> ExchangeRefreshToken()
    {
        if (!Request.Cookies.TryGetValue("refreshToken", out var refreshToken))
            return Unauthorized(new { message = "No refresh token" });
        
        var tokens = await authService.ExchangeRefreshToken(refreshToken);
        if (tokens == null)
            return Unauthorized(new { message = "Invalid or expired refresh token" });
        
        Response.Cookies.Append("refreshToken", tokens.RefreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
        });

        return Ok(tokens);
    }
    
    [HttpPost("googleLogin")]
    public async Task<ActionResult<AuthResponseDto>> GoogleLogin([FromBody] GoogleLoginRequest request)
    {
        var tokens = await authService.GoogleLogin(request.AuthCode);
        
        if (tokens == null)
            return BadRequest("Failed to exchange auth code.");
        
        Response.Cookies.Append("refreshToken", tokens.RefreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
        });
        
        return Ok(tokens);
    }

    [HttpPost("signup")]
    public async Task<ActionResult> SignUp([FromBody] SignUpRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
            return BadRequest("Email must be provided.");
        
        if (string.IsNullOrWhiteSpace(request.Password))
            return BadRequest("Password must be provided.");
        
        if (!IsValidEmail(request.Email))
            return BadRequest("Invalid email address.");

        throw new NotImplementedException();
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
        
        throw new NotImplementedException();
    }
    
    private static bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        try
        {
            // Normalize the domain
            email = Regex.Replace(email, @"(@)(.+)$", DomainMapper,
                RegexOptions.None, TimeSpan.FromMilliseconds(200));

            // Examines the domain part of the email and normalizes it.
            string DomainMapper(Match match)
            {
                // Use IdnMapping class to convert Unicode domain names.
                var idn = new IdnMapping();

                // Pull out and process domain name (throws ArgumentException on invalid)
                string domainName = idn.GetAscii(match.Groups[2].Value);

                return match.Groups[1].Value + domainName;
            }
        }
        catch (RegexMatchTimeoutException e)
        {
            return false;
        }
        catch (ArgumentException e)
        {
            return false;
        }

        try
        {
            return Regex.IsMatch(email,
                @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
                RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));
        }
        catch (RegexMatchTimeoutException)
        {
            return false;
        }
    }
}