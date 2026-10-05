using System.ComponentModel.DataAnnotations;

namespace GameBackend.Auth.HalloGames.Auth.Api.Models.Requests
{
    public class RefreshTokenRequest
    {
        [Required] public string RefreshToken { get; set; } = string.Empty;
    }
}
