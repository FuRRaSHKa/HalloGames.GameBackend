using GameBackend.HalloGames.Auth.Api.Models.Credentials;
using GameBackend.HalloGames.Auth.Api.Models.Results;
using GameBackend.HalloGames.Users.Models;

namespace GameBackend.HalloGames.Auth.Api.AuthProviders
{
    internal interface IAuthProvider<TCredentials> where TCredentials : AuthCredentials
    {
        public Task<User?> Login(TCredentials credentials);
        public Task<User?> Register(TCredentials credentials);
    }
}
