namespace GameBackend.Controllers.HalloGames.Auth.Api.Models.Requests
{
    public sealed record EmailRegisterRequest
    (
        string Email, string Password
    );
}
