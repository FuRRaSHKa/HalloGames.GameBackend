using GameBackend.Controllers.HalloGames.Auth.Api.AuthProviders;
using GameBackend.Controllers.HalloGames.Auth.Api.Models.Results;

namespace GameBackend.Controllers.HalloGames.Auth.Api.Services
{
    internal class AuthSystem
    {
        private readonly IAuthProviderSelector _providerSelector;


        public async Task<AuthResult?> Login<TCredentials>()
        {
            return null;
        } 
    }
}
