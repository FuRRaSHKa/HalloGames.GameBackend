namespace GameBackend.HalloGames.Auth.Api.Models.Results
{
    internal sealed record TokenResult(
        string AccessToken,
        string RefreshToken);
}
