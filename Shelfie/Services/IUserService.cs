
using Shelfie.Models;

namespace Shelfie.Services;

public interface IUserService
{
    public Task<User?> GetUserByEmail(string email);
    public Task<User?> GetUserByName(string userName);
    public Task<string?> GoogleLogin(string authCode);
    public Task<bool> Register(string email, string username);
    public Task SignUp(string email, string password);
}