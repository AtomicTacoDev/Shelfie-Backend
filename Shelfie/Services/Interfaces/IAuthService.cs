
using Shelfie.Models.Dto;

namespace Shelfie.Services;

public interface IAuthService
{
    public Task<UserDto?> GetUserByEmail(string email);
    public Task<UserDto?> GetUserByName(string userName);
    public Task<AuthResponseDto?> ExchangeRefreshToken(string refreshToken);
    public Task<AuthResponseDto?> Login(string email, string password);
    public Task<AuthResponseDto?> GoogleLogin(string authCode);
    public Task<string?> Register(string email, string username);
    public Task RevokeRefreshToken(string refreshToken);
    public Task<SignupResponseDto> CreatePendingSignup(string email, string username, string password);
    public Task<AuthResponseDto?> ConfirmEmailAndCreateUser(string confirmationToken);
    public Task<SignupResponseDto> ResendConfirmationEmail(string email);
}