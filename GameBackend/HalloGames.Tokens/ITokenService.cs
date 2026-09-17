using GameBackend.HalloGames.Users.Models;

namespace GameBackend.HalloGames.Tokens
{
    public interface ITokenService
    {
        public string GenerateAccesToken(User user);
        public Task<string> GenerateRefreshToken(User user);
    }
}
