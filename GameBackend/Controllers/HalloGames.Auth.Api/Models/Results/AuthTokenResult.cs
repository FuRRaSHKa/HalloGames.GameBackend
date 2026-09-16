namespace GameBackend.Controllers.HalloGames.Auth.Api.Models.Results
{
    internal sealed record AuthTokenResult(
     string AccessToken,
     string RefreshToken,
     DateTime AccessTokenExpiresAt);
}
