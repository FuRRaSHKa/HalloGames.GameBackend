using GameBackend.HalloGames.Tokens.Data;
using GameBackend.HalloGames.Users.Models;

namespace GameBackend.HalloGames.Tokens
{
    internal class TokenService : ITokenService
    {
        private readonly IConfiguration _configuration;
        private readonly RefreshTokenDbContext _refreshTokenBdContext;

        internal TokenService(IConfiguration configuration, RefreshTokenDbContext refreshTokenBdContext)
        {
            _configuration = configuration;
            _refreshTokenBdContext = refreshTokenBdContext; 
        }

        public string GenerateAccesToken(User user)
        {
            throw new NotImplementedException();
        }

        public Task<string> GenerateRefreshToken(User user)
        {
            return null;
        }
    }
}
