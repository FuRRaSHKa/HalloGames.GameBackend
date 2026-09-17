using GameBackend.HalloGames.Auth.Api.Models.Credentials;

namespace GameBackend.HalloGames.Auth.Api.AuthProviders
{
    internal interface IAuthProviderSelector
    {
        public IAuthProvider<TCredentials> GetAuthProvider<TCredentials>() where TCredentials : AuthCredentials;
    }
}
