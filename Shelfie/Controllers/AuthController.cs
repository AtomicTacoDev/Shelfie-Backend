using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shelfie.Models.Dto;
using Shelfie.Services;
using JwtRegisteredClaimNames = System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames;

namespace Shelfie.Controllers;

[ApiController]
[Route("[controller]")]
public partial class AuthController(IAuthService authService, IEmailService emailService) : ControllerBase
{
    public record GoogleLoginRequest(string AuthCode);
    public record RegisterRequest(string Username);
    public record SignUpRequest(string Email, string Username, string Password);
    public record ConfirmEmailRequest(string Token);
    public record LoginRequest(string Email, string Password);
    public record ResendConfirmationRequest(string Email);
    
    [GeneratedRegex("^[a-zA-Z0-9]+$")]
    private static partial Regex UsernameRegex();
    
    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<UserDto>> Me()
    {
        var email = User.FindFirst(c => c.Type == ClaimTypes.Email)!.Value;

        var user = await authService.GetUserByEmail(email);
        if (user == null || string.IsNullOrEmpty(user.UserName))
        {
            return NotFound();
        }
        
        return Ok(user);
    }
    
    [HttpPost("refresh")]
    public async Task<ActionResult<AuthResponseDto>> ExchangeRefreshToken()
    {
        if (!Request.Cookies.TryGetValue("refreshToken", out var refreshToken))
        {
            return Unauthorized(new { message = "No refresh token" });
        }
        
        var tokens = await authService.ExchangeRefreshToken(refreshToken);
        if (tokens == null)
        {
            return Unauthorized(new { message = "Invalid or expired refresh token" });
        }
        
        Response.Cookies.Append("refreshToken", tokens.RefreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
        });

        return Ok(tokens);
    }
    
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        if (Request.Cookies.TryGetValue("refreshToken", out var refreshToken))
        {
            await authService.RevokeRefreshToken(refreshToken);
            
            Response.Cookies.Delete("refreshToken", new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
            });
        }
        
        return Ok();
    }
    
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
            return BadRequest("Email must be provided.");
    
        if (string.IsNullOrWhiteSpace(request.Password))
            return BadRequest("Password must be provided.");

        var tokens = await authService.Login(request.Email, request.Password);
        
        if (tokens == null)
            return Unauthorized("Invalid email or password.");
        
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
        {
            return BadRequest("Failed to exchange auth code.");
        }
        
        if (tokens.RefreshToken != null)
        {
            Response.Cookies.Append("refreshToken", tokens.RefreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
            });
        }
        
        return Ok(tokens);
    }

    [HttpPost("signup")]
    public async Task<ActionResult> SignUp([FromBody] SignUpRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
            return BadRequest("Email must be provided.");
    
        if (string.IsNullOrWhiteSpace(request.Password))
            return BadRequest("Password must be provided.");
    
        if (string.IsNullOrWhiteSpace(request.Username))
            return BadRequest("Username must be provided.");
    
        if (!IsValidEmail(request.Email))
            return BadRequest("Invalid email address.");
    
        var username = request.Username.Trim();
    
        if (username.Length is < 3 or > 20)
            return BadRequest("Username must be between 3 and 20 characters.");

        if (!UsernameRegex().IsMatch(username))
            return BadRequest("Username can only contain letters and numbers.");

        var result = await authService.CreatePendingSignup(request.Email, request.Username, request.Password);
    
        if (!result.Success)
            return BadRequest(result.Message);
        
        var emailResult = await emailService.SendEmailConfirmationAsync(
            request.Email, 
            request.Username, 
            result.ConfirmationToken!);
    
        return Ok(new { message = "Please check your email to confirm your account." });
    }
    
    [HttpPost("confirm-email")]
    public async Task<ActionResult> ConfirmEmail([FromBody] ConfirmEmailRequest request)
    {
        var result = await authService.ConfirmEmailAndCreateUser(request.Token);
    
        if (result == null)
            return BadRequest("Invalid or expired confirmation token.");
        
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(result.Jwt);
        var email = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Email)?.Value;
        var username = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
        
        if (!string.IsNullOrEmpty(email) && !string.IsNullOrEmpty(username))
        {
            _ = emailService.SendWelcomeEmailAsync(email, username);
        }
    
        return Ok(new { message = "Email confirmed successfully. Please log in." });
    }
    
    [HttpPost("resend-confirmation")]
    public async Task<ActionResult> ResendConfirmation([FromBody] ResendConfirmationRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
            return BadRequest("Email must be provided.");
        
        var result = await authService.ResendConfirmationEmail(request.Email);
        
        if (!result.Success)
            return BadRequest(result.Message);
        
        var emailResult = await emailService.SendEmailConfirmationAsync(
            request.Email,
            result.Username!,
            result.ConfirmationToken!);
        
        if (!emailResult.Success)
        {
            return StatusCode(500, "Failed to send confirmation email. Please try again later.");
        }
        
        return Ok(new { message = "Confirmation email has been resent. Please check your inbox." });
    }
    
    [Authorize]
    [HttpPost("register")]
    public async Task<ActionResult<UserDto>> Register([FromBody] RegisterRequest request)
    {
        var username = request.Username.Trim();
        
        if (string.IsNullOrWhiteSpace(username))
        {
            return BadRequest("Username cannot be empty.");
        }
        
        if (username.Length is < 3 or > 20)
        {
            return BadRequest("Username must be between 3 and 20 characters.");
        }

        if (!UsernameRegex().IsMatch(username))
        {
            return BadRequest("Username can only contain letters and numbers.");
        }
        
        var email = User.FindFirst(ClaimTypes.Email)?.Value 
                    ?? User.FindFirst(JwtRegisteredClaimNames.Email)?.Value;

        if (string.IsNullOrEmpty(email))
        {
            return BadRequest("Email not found in token.");
        }

        var refreshToken = await authService.Register(email, username);
        
        if (refreshToken is null)
        {
            return BadRequest("Username already taken.");
        }
        
        var user = await authService.GetUserByEmail(email);
        return Ok(user);
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