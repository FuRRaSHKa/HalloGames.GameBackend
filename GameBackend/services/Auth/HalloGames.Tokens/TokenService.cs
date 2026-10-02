using GameBackend.HalloGames.Tokens.Data;
using GameBackend.HalloGames.Tokens.Models;
using GameBackend.HalloGames.Time;
using GameBackend.HalloGames.Users.Data;
using GameBackend.HalloGames.Users.Models;
using GameBackend.HalloGames.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace GameBackend.HalloGames.Tokens
{
    internal class TokenService : ITokenService
    {
        private readonly IConfiguration _configuration;
        private readonly RefreshTokenDbContext _refreshTokenBdContext;
        private readonly UserDbContext _userDbContext;
        private readonly ITimeService _timeService;

        internal TokenService(IConfiguration configuration, RefreshTokenDbContext refreshTokenBdContext, UserDbContext userDbContext, ITimeService timeService)
        {
            _configuration = configuration;
            _refreshTokenBdContext = refreshTokenBdContext;
            _userDbContext = userDbContext;
            _timeService = timeService;
        }

        public async Task<string> GenerateAccesToken(User user)
        {
            var jwtSettings = _configuration.GetSection("Jwt");
            var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]!);

            var claims = new[]
            {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Name, user.Username),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
        };

            var creds = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: _timeService.UtcNow.AddMinutes(double.Parse(jwtSettings["AccessTokenExpirationMinutes"]!)),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<string> GenerateAccesToken(string refreshToken)
        {
            var tokenHash = HashUtils.HashRefrestToken(refreshToken);
            var refreshTokens = await _refreshTokenBdContext.RefreshTokens.FirstOrDefaultAsync(f => f.Token == tokenHash);
            
            if(refreshTokens == null || refreshTokens.IsRevoked || refreshTokens.Expires <= _timeService.UtcNow)
            {
                return null;
            }

            var user = await _userDbContext.Users.FirstOrDefaultAsync(u => u.Id == refreshTokens.UserId);
            if(user == null)
            {
                return null;
            }

            return await GenerateAccesToken(user);
        }

        public async Task<string> GenerateRefreshToken(User user)
        {
            var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
            var tokenHash = HashUtils.HashRefrestToken(token);

            var refreshToken = new RefreshToken
            {
                Token = tokenHash,
                UserId = user.Id,
                Expires = _timeService.UtcNow.AddDays(
                double.Parse(_configuration["Jwt:RefreshTokenExpirationDays"]!))
            };

            _refreshTokenBdContext.RefreshTokens.Add(refreshToken);
            await _refreshTokenBdContext.SaveChangesAsync();
            return token;
        }
    }
}
