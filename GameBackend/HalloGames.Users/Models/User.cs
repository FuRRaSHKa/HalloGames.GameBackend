using GameBackend.HalloGames.Auth.Api.AuthProviders;

namespace GameBackend.HalloGames.Users.Models
{
    public class User
    {
        public Guid Id { get; set; }
        public AuthType AuthType { get; init; }
        public string ExterntalId { get; init; } = string.Empty;
    }
}
