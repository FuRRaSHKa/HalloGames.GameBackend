using GameBackend.HalloGames.Auth.Api.AuthProviders;

namespace GameBackend.HalloGames.Auth.Api.Models
{
    public record UserAuthIdentity(AuthType AuthType, string ExternalId);
}
