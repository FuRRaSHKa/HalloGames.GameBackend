using GameBackend.HalloGames.Auth.Api.Models.Credentials;
using GameBackend.HalloGames.Users.Models;

namespace GameBackend.HalloGames.Auth.Api.AuthProviders
{
    internal abstract class BaseAuthProvider<TCredentials> : IAuthProvider<TCredentials>
        where TCredentials : AuthCredentials
    {
        public Task<User?> Login(TCredentials credentials)
        {
            throw new NotImplementedException();
        }

        public Task<User?> Register(TCredentials credentials)
        {
            throw new NotImplementedException();
        }
    }
}
