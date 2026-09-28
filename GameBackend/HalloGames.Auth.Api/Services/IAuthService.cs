using GameBackend.HalloGames.Auth.Api.Models.Credentials;
using GameBackend.HalloGames.Auth.Api.Models.Results;
using GameBackend.HalloGames.Users.Models;

namespace GameBackend.HalloGames.Auth.Api.Services
{
    public interface IAuthService
    {
        Task<TokenResult?> Login<TCredentials>(TCredentials credentials) where TCredentials : AuthCredentials;
        Task<User?> Register<TCredentials>(TCredentials credentials) where TCredentials : AuthCredentials;
        Task<TokenResult?> RefreshTokenAsync(string refreshToken);
    }
}
