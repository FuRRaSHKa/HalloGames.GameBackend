using GameBackend.HalloGames.Auth.Api.AuthProviders;

namespace GameBackend.HalloGames.Auth.Api.Models.Identities
{
    public class UserAuthIdentity
    {
        public Guid Id { get; set; }    
        public AuthType AuthType { get; set;  }
        public Guid UserId { get; set; }
        public string ExternalId { get; set; } = string.Empty; 
    }
}
