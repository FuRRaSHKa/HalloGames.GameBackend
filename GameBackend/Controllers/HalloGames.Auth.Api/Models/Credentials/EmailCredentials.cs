namespace GameBackend.Controllers.HalloGames.Auth.Api.Models.Credentials
{
    internal record EmailCredentials
        (
            string email,
            string password
        ) : AuthCredentials;
}
