namespace GameBackend.HalloGames.Auth.Api.Models.Credentials
{
    public record PasswordCredentials
        (
            string Email,
            string Password
        ) : AuthCredentials;
}
