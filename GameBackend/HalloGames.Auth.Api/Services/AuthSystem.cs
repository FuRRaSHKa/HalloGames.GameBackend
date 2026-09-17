using GameBackend.HalloGames.Auth.Api.AuthProviders;
using GameBackend.HalloGames.Auth.Api.Models.Credentials;
using GameBackend.HalloGames.Auth.Api.Models.Results;
using GameBackend.HalloGames.Tokens;
using GameBackend.HalloGames.Users;
using GameBackend.HalloGames.Users.Models;

namespace GameBackend.HalloGames.Auth.Api.Services
{
    internal class AuthSystem
    {
        private readonly IAuthProviderSelector _providerSelector;
        private readonly ITokenService _tokenService;
        private readonly IUserRepository _userRepository;

        internal AuthSystem(IAuthProviderSelector authProviderSelector, ITokenService tokenService)
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

            var accesToken = _tokenService.GenerateAccesToken(identity);
            var refreshToken = await _tokenService.GenerateRefreshToken(identity);
        
            return new TokenResult(accesToken, refreshToken);
        }

        public async Task<User?> Register<TCredentials>(TCredentials credentials) where TCredentials : AuthCredentials
        {
            var provider = _providerSelector.GetAuthProvider<TCredentials>();
            var identity = await provider.Register(credentials);
            if (identity == null)
                return null;

            var user = new User() 
            {
                AuthType = identity.AuthType,
                ExterntalId = identity.ExterntalId,
            };

            await _userRepository.Register(user);
            return user;
        }

        public async Task<TokenResult?> RefreshTokenAsync(string refreshToken)
        {
            return null;
        }
    }
}
