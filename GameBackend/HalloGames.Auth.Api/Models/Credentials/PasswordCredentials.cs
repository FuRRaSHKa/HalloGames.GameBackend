namespace GameBackend.HalloGames.Auth.Api.Models.Credentials
{
    internal record PasswordCredentials
        (
            string Email,
            string Password
        ) : AuthCredentials;
}
