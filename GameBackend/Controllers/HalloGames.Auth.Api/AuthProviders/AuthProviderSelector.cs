using GameBackend.Controllers.HalloGames.Auth.Api.Models.Credentials;

namespace GameBackend.Controllers.HalloGames.Auth.Api.AuthProviders
{
    internal class AuthProviderSelector : IAuthProviderSelector
    {
        internal IAuthProvider GetProvider<TCredentials>() where TCredentials : AuthCredentials
        {
            
        }
    }
}
