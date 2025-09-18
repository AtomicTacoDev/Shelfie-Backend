using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace Shelfie.Services;

public class UserService(IConfiguration config) : IUserService
{
    public bool ValidateToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(config["Jwt:Key"] ?? throw new InvalidOperationException());

        try
        {
            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = config["Jwt:Issuer"],
                ValidateAudience = true,
                ValidAudience = config["Jwt:Audience"],
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            }, out _);

            return true;
        }
        catch
        {
            return false;
        }
    }
    
    public async Task<string?> GoogleLogin(string authCode)
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
                var errorBody = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Token request failed: {response.StatusCode} {errorBody}");
                return null;
            }
            
            response.EnsureSuccessStatusCode();
            
            var tokenResponse = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>();
            var accessToken = tokenResponse?["access_token"].ToString();
            
            var userResponse = await client.GetAsync($"https://www.googleapis.com/oauth2/v2/userinfo?access_token={accessToken}");
            userResponse.EnsureSuccessStatusCode();
            var userInfo = await userResponse.Content.ReadFromJsonAsync<Dictionary<string, object>>();
            
            var userId = userInfo?["id"].ToString();
            
            var jwt = GenerateJwt(userId);

            return jwt;
        }
        catch
        {
            return null;
        }
    }
    
    private string GenerateJwt(string userId)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: config["Jwt:Issuer"],
            audience: config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}