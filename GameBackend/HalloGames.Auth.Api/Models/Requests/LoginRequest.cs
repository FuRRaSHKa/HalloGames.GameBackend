namespace GameBackend.HalloGames.Auth.Api.Models.Requests
{
    public sealed record EmailLoginRequest
    (
        string Email, string Password
    );
}
