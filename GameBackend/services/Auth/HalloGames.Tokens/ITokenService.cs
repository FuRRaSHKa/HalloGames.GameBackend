using GameBackend.HalloGames.Users.Models;

namespace GameBackend.HalloGames.Tokens
{
    public interface ITokenService
    {
        public Task<string> GenerateAccesToken(User user);
        public Task<string> GenerateAccesToken(string refreshToken);
        public Task<string> GenerateRefreshToken(User user);
    }
}
