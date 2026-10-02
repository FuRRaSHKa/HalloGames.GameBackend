using GameBackend.HalloGames.Auth.Api.Models.Credentials;

namespace GameBackend.HalloGames.Auth.Api.AuthProviders
{
    internal class AuthProviderSelector : IAuthProviderSelector
    {
        private readonly IServiceProvider _services;

        public AuthProviderSelector(IServiceProvider services)
        {
            _services = services;
        }

        public IAuthProvider<TCredentials> GetAuthProvider<TCredentials>()
            where TCredentials : AuthCredentials
        {
            return _services.GetRequiredService<IAuthProvider<TCredentials>>();
        }
    }
}
