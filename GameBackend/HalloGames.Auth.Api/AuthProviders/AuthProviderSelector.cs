using GameBackend.HalloGames.Auth.Api.Models.Credentials;

namespace GameBackend.HalloGames.Auth.Api.AuthProviders
{
    internal class AuthProviderSelector : IAuthProviderSelector
    {
        public IAuthProvider<TCredentials> GetAuthProvider<TCredentials>() where TCredentials : AuthCredentials
        {
            return null;
        }
    }
}
