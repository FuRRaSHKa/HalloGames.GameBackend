namespace GameBackend.HalloGames.Auth.Api.Models.Results
{
    public sealed record TokenResult(
        string AccessToken,
        string RefreshToken);
}
