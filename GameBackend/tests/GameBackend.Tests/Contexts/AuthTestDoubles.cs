using GameBackend.HalloGames.Auth.Api.AuthProviders;
using GameBackend.HalloGames.Auth.Api.Models.Credentials;
using GameBackend.HalloGames.Tokens;
using GameBackend.HalloGames.Users.Models;

namespace GameBackend.Tests.Contexts;

internal sealed class StubPasswordAuthProvider(User? user) : IAuthProvider<PasswordCredentials>
{
    public User? LoginResult { get; set; } = user;
    public User? RegisterResult { get; set; }
    public List<PasswordCredentials> LoginCalls { get; } = [];
    public List<PasswordCredentials> RegisterCalls { get; } = [];

    public Task<User?> Login(PasswordCredentials credentials)
    {
        LoginCalls.Add(credentials);
        return Task.FromResult(LoginResult);
    }

    public Task<User?> Register(PasswordCredentials credentials)
    {
        RegisterCalls.Add(credentials);
        return Task.FromResult(RegisterResult);
    }
}

internal sealed class StubAuthProviderSelector(IAuthProvider<PasswordCredentials> provider)
    : IAuthProviderSelector
{
    public IAuthProvider<TCredentials> GetAuthProvider<TCredentials>()
        where TCredentials : AuthCredentials
    {
        return provider as IAuthProvider<TCredentials>
            ?? throw new NotSupportedException("Only password credentials are expected in this test.");
    }
}

internal sealed class StubTokenService : ITokenService
{
    public List<User> AccessTokenCalls { get; } = [];
    public List<User> RefreshTokenCalls { get; } = [];

    public Task<string> GenerateAccesToken(User user)
    {
        AccessTokenCalls.Add(user);
        return Task.FromResult("test-access-token");
    }

    public Task<string> GenerateRefreshToken(User user)
    {
        RefreshTokenCalls.Add(user);
        return Task.FromResult("test-refresh-token");
    }

    public Task<string?> GenerateAccesToken(string refreshToken) =>
        throw new NotSupportedException("Token refresh is not expected in this test.");
}
