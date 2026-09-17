namespace GameBackend.HalloGames.Auth.Api.Models.Credentials
{
    internal record EmailCredentials
        (
            string Email,
            string Password
        ) : AuthCredentials;
}
