
using Microsoft.AspNetCore.Mvc;
using Google.Apis.Auth;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Shelfie.Controllers;

public record GoogleToken(string Credential);

[ApiController]
[Route("[controller]")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _config;

    public AuthController(IConfiguration config)
    {
        _config = config;
    }

    [HttpGet("validateToken")]
    public IActionResult ValidateToken()
    {
        var authHeader = HttpContext.Request.Headers["Authorization"].ToString();

        if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
            return Unauthorized();

        var token = authHeader.Substring("Bearer ".Length).Trim();

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_config["Jwt:Key"]);

        try
        {
            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _config["Jwt:Issuer"],
                ValidateAudience = true,
                ValidAudience = _config["Jwt:Audience"],
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            }, out _);

            return Ok();
        }
        catch
        {
            return BadRequest();
        }
    }
    
    [HttpPost("googleLogin")]
    public async Task<IActionResult> GoogleLogin([FromBody] GoogleToken token)
    {
        try
        {
            var payload = await GoogleJsonWebSignature.ValidateAsync(token.Credential);
            var jwt = GenerateJwt(payload.Subject);

            return Ok(new
            {
                token = jwt
            });
        }
        catch (Exception e)
        {
            return BadRequest(new { error = "Invalid token.", details = e.Message });
        }
    }
    
    private string GenerateJwt(string userId)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}