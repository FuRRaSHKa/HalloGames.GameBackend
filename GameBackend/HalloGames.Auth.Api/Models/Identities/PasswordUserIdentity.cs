namespace GameBackend.HalloGames.Auth.Api.Models.Identities
{
    public class PasswordUserIdentity : UserAuthIdentity
    {
        public string PasswordHash { get; set; } = string.Empty;
    }
}
