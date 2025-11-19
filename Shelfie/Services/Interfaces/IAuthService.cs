
using Shelfie.Models.Dto;

namespace Shelfie.Services;

public interface IAuthService
{
    public Task<UserDto?> GetUserByEmail(string email);
    public Task<UserDto?> GetUserByName(string userName);
    public Task<AuthResponseDto?> ExchangeRefreshToken(string refreshToken);
    public Task<AuthResponseDto?> GoogleLogin(string authCode);
    //public Task<bool> Register(string email, string username);
    //public Task SignUp(string email, string password);
}