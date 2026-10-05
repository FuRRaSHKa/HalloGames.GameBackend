using GameBackend.HalloGames.Auth.Api.AuthProviders;
using GameBackend.HalloGames.Auth.Api.Models.Credentials;
using GameBackend.HalloGames.Auth.Api.Models.Results;
using GameBackend.HalloGames.Tokens;
using GameBackend.HalloGames.Users;
using GameBackend.HalloGames.Users.Data;
using GameBackend.HalloGames.Users.Models;

namespace GameBackend.HalloGames.Auth.Api.Services
{
    internal class AuthService : IAuthService
    {
        private readonly IAuthProviderSelector _providerSelector;
        private readonly ITokenService _tokenService;

        internal AuthService(IAuthProviderSelector authProviderSelector, ITokenService tokenService, UserDbContext userDbContext)
        {
            _providerSelector = authProviderSelector;
            _tokenService = tokenService;
        }

        public async Task<TokenResult?> Login<TCredentials>(TCredentials credentials) where TCredentials : AuthCredentials
        {
            var provider = _providerSelector.GetAuthProvider<TCredentials>();
            var identity = await provider.Login(credentials);
            if (identity == null)
                return null;

            var accesToken = await _tokenService.GenerateAccesToken(identity);
            var refreshToken = await _tokenService.GenerateRefreshToken(identity);
        
            return new TokenResult(accesToken, refreshToken);
        }

        public async Task<User?> Register<TCredentials>(TCredentials credentials) where TCredentials : AuthCredentials
        {
            var provider = _providerSelector.GetAuthProvider<TCredentials>();
            var user = await provider.Register(credentials);
            if (user == null)
                return null;

            return user;
        }

        public async Task<TokenResult?> RefreshTokenAsync(string refreshToken)
        {
            var accesToken = await _tokenService.GenerateAccesToken(refreshToken);
            if(accesToken == null)
                return null;    

            return new TokenResult(accesToken, refreshToken);
        }
    }
}
