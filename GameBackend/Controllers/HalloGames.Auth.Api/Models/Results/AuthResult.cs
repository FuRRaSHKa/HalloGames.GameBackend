namespace GameBackend.Controllers.HalloGames.Auth.Api.Models.Results
{
    internal sealed record AuthResult(
        Guid UserId,
        AuthTokenResult Tokens);
}
