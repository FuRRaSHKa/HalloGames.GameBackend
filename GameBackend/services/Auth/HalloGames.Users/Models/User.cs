using GameBackend.HalloGames.Auth.Api.AuthProviders;

namespace GameBackend.HalloGames.Users.Models
{
    public class User
    {
        public Guid Id { get; set; }
        public string Username { get; init; } = string.Empty;
    }
}
