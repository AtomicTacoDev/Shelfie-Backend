using Microsoft.AspNetCore.Mvc;

namespace Shelfie.Services;

public interface IUserService
{
    public bool ValidateToken(string token);
    public Task<string?> GoogleLogin(string authCode);
}