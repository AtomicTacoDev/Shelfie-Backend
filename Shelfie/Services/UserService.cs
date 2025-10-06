
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Shelfie.Models;

namespace Shelfie.Services;

public class UserService(IConfiguration config, UserManager<User> userManager) : IUserService
{
    public async Task<User?> GetUser(string email)
    {
        var user = await userManager.FindByEmailAsync(email);

        return user;
    }

    public async Task<bool> Register(string email, string username)
    {
        var user = new User
        {
            Email = email,
            UserName = username
        };

        var result = await userManager.CreateAsync(user);

        return result.Succeeded;
    }

    public Task SignUp(string email, string password)
    {
        throw new NotImplementedException();
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
            
            var email = userInfo?["email"].ToString();
            var jwt = GenerateJwt(email);
            
            return jwt;
        }
        catch
        {
            return null;
        }
    }
    
    private string GenerateJwt(string email)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Email, email),
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