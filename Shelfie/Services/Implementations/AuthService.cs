using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Shelfie.Data;
using Shelfie.Data.Models;
using Shelfie.Models;
using Shelfie.Models.Dto;

namespace Shelfie.Services;

public class AuthService(IConfiguration config, UserManager<User> userManager, ApplicationDbContext dbContext) : IAuthService
{
    public async Task<UserDto?> GetUserByEmail(string email)
    {
        var identityUser = await userManager.FindByEmailAsync(email);
        
        return identityUser is null ? null : new UserDto(identityUser.Id, identityUser.UserName, identityUser.Email);
    }
    
    public async Task<UserDto?> GetUserByName(string userName)
    {
        var identityUser = await userManager.FindByNameAsync(userName);

        return identityUser is null ? null : new UserDto(identityUser.Id, identityUser.UserName, identityUser.Email);
    }
    
    public async Task<UserDto?> GetUserById(string id)
    {
        var identityUser = await userManager.FindByIdAsync(id);

        return identityUser is null ? null : new UserDto(identityUser.Id, identityUser.UserName, identityUser.Email);
    }

    public async Task<string?> Register(string email, string username)
    {
        var existingUser = await userManager.FindByNameAsync(username);
        if (existingUser != null)
        {
            return null;
        }
        
        var user = new User
        {
            Email = email,
            UserName = username,
            EmailConfirmed = true
        };
        
        var createResult = await userManager.CreateAsync(user);
        
        if (!createResult.Succeeded)
        {
            return null;
        }
         
        dbContext.Libraries.Add(new Library
        {
            UserId = user.Id,
            Objects = new List<PlacedObject>()
        });
        
        var refreshToken = GenerateRefreshToken();
        dbContext.RefreshTokens.Add(new RefreshToken
        {
            UserId = user.Id,
            Token = refreshToken,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(int.Parse(config["Jwt:RefreshTokenExpiryDays"]!)),
            IsRevoked = false
        });
        
        await dbContext.SaveChangesAsync();

        return refreshToken;
    }

    public async Task<AuthResponseDto?> GoogleLogin(string authCode)
    {
        try
        {
            var values = new Dictionary<string, string>
            {
                { "code", authCode },
                { "client_id", config["Google:ClientId"] },
                { "client_secret", config["Google:ClientSecret"] },
                { "redirect_uri", config["Google:RedirectUri"] },
                { "grant_type", "authorization_code" }
            };

            using var client = new HttpClient();
            var response = await client.PostAsync("https://oauth2.googleapis.com/token", new FormUrlEncodedContent(values));
            
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }
            
            var tokenResponse = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>();
            var accessToken = tokenResponse?["access_token"].ToString();
            
            var userResponse = await client.GetAsync($"https://www.googleapis.com/oauth2/v2/userinfo?access_token={accessToken}");
            userResponse.EnsureSuccessStatusCode();
            var userInfo = await userResponse.Content.ReadFromJsonAsync<Dictionary<string, object>>();
            
            var email = userInfo?["email"].ToString();
            
            var user = await userManager.FindByEmailAsync(email);
            
            var jwt = await GenerateJwt(email);
            
            if (user == null || string.IsNullOrEmpty(user.UserName))
            {
                return new AuthResponseDto(jwt, null);
            }
            
            var refreshToken = GenerateRefreshToken();
            var refreshTokenEntity = new RefreshToken
            {
                UserId = user.Id,
                Token = refreshToken,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(int.Parse(config["Jwt:RefreshTokenExpiryDays"]!)),
                IsRevoked = false
            };
                
            dbContext.RefreshTokens.Add(refreshTokenEntity);
            await dbContext.SaveChangesAsync();
            
            return new AuthResponseDto(jwt, refreshToken);
        }
        catch
        {
            return null;
        }
    }
    
    public async Task<AuthResponseDto?> ExchangeRefreshToken(string refreshToken)
    {
        var tokenEntity = await dbContext.RefreshTokens
            .FirstOrDefaultAsync(t => t.Token == refreshToken && !t.IsRevoked);

        if (tokenEntity == null || tokenEntity.ExpiresAt < DateTime.UtcNow)
            return null;

        var user = await GetUserById(tokenEntity.UserId);
        if (user == null)
            return null;
        
        tokenEntity.IsRevoked = true;

        var newRefreshToken = GenerateRefreshToken();
        var newRefreshTokenEntity = new RefreshToken
        {
            UserId = user.Id,
            Token = newRefreshToken,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(int.Parse(config["Jwt:RefreshTokenExpiryDays"]!)),
            IsRevoked = false
        };

        dbContext.RefreshTokens.Add(newRefreshTokenEntity);
        await dbContext.SaveChangesAsync();

        var jwt = await GenerateJwt(user.Email);

        return new AuthResponseDto(jwt, newRefreshToken);
    }
    
    public async Task RevokeRefreshToken(string refreshToken)
    {
        var tokenEntity = await dbContext.RefreshTokens
            .FirstOrDefaultAsync(t => t.Token == refreshToken);

        if (tokenEntity != null)
        {
            tokenEntity.IsRevoked = true;
            await dbContext.SaveChangesAsync();
        }
    }
    
    private async Task<string> GenerateJwt(string email)
    {
        var user = await userManager.FindByEmailAsync(email);
        
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Email, email)
        };
        
        if (user != null && !string.IsNullOrEmpty(user.UserName))
        {
            claims.Add(new Claim(ClaimTypes.Name, user.UserName));
        }

        var token = new JwtSecurityToken(
            issuer: config["Jwt:Issuer"],
            audience: config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
    
    private string GenerateRefreshToken()
    {
        var randomBytes = new byte[64];
        RandomNumberGenerator.Create().GetBytes(randomBytes);
        
        return Convert.ToBase64String(randomBytes);
    }
}