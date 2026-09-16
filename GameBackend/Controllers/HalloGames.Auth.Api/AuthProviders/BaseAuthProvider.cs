using GameBackend.Controllers.HalloGames.Auth.Api.Models.Credentials;

namespace GameBackend.Controllers.HalloGames.Auth.Api.AuthProviders
{
    internal abstract class BaseAuthProvider<TCredentials> : IAuthProvider
        where TCredentials : AuthCredentials
    {

    }
}
