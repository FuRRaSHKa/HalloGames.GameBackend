using GameBackend.HalloGames.Auth.Api.AuthProviders;
using GameBackend.HalloGames.Auth.Api.Data;
using GameBackend.HalloGames.Auth.Api.Models.Credentials;
using GameBackend.HalloGames.Auth.Api.Models.Identities;
using GameBackend.HalloGames.Auth.Api.Services;
using GameBackend.HalloGames.Tokens;
using GameBackend.HalloGames.Users.Data;
using GameBackend.HalloGames.Users.Models;
using Microsoft.EntityFrameworkCore;

namespace GameBackend.Tests;

public class AuthServiceTests
{
    [Fact]
    public async Task Login_WhenPasswordIsIncorrect_ReturnsNullWithoutGeneratingTokens()
    {
        const string email = "player@example.com";
        const string correctPassword = "correct-password";
        var user = new User { Id = Guid.NewGuid(), Username = "Player" };
        using var passwordDbContext = new PasswordDbContext(
            new DbContextOptionsBuilder<PasswordDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options);

        passwordDbContext.Users.Add(user);
        passwordDbContext.Passwords.Add(new PasswordUserIdentity
        {
            UserId = user.Id,
            ExternalId = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(correctPassword)
        });

        await passwordDbContext.SaveChangesAsync();
        passwordDbContext.ChangeTracker.Clear();

        IAuthProvider<PasswordCredentials> provider = new PasswordAuthProvider(passwordDbContext);
        // Проверяем, что учетная запись доступна и правильный пароль принимается.
        var authenticatedUser = await provider.Login(new PasswordCredentials(email, correctPassword));
        Assert.NotNull(authenticatedUser);
        Assert.Equal(user.Id, authenticatedUser.Id);

        var tokenService = new StubTokenService();
        using var dbContext = new UserDbContext(new DbContextOptionsBuilder<UserDbContext>().Options);
        var service = new AuthService(new StubAuthProviderSelector(provider), tokenService, dbContext);

        var result = await service.Login(new PasswordCredentials(email, "wrong-password"));

        Assert.Null(result);
        Assert.Empty(tokenService.AccessTokenCalls);
        Assert.Empty(tokenService.RefreshTokenCalls);
    }

    [Fact]
    public async Task Login_WhenCredentialsAreValid_ReturnsTokensForAuthenticatedUser()
    {
        var credentials = new PasswordCredentials("player@example.com", "test-password");
        var user = new User { Id = Guid.NewGuid(), Username = "Player" };
        var provider = new StubPasswordAuthProvider(user);
        var selector = new StubAuthProviderSelector(provider);
        var tokenService = new StubTokenService();

        using var dbContext = new UserDbContext(new DbContextOptionsBuilder<UserDbContext>().Options);
        var service = new AuthService(selector, tokenService, dbContext);

        var result = await service.Login(credentials);

        Assert.NotNull(result);
        Assert.Equal("test-access-token", result.AccessToken);
        Assert.Equal("test-refresh-token", result.RefreshToken);
        Assert.Same(credentials, Assert.Single(provider.LoginCalls));
        Assert.Same(user, Assert.Single(tokenService.AccessTokenCalls));
        Assert.Same(user, Assert.Single(tokenService.RefreshTokenCalls));
    }

    [Theory]
    [InlineData(2)]
    [InlineData(3)]
    public async Task Login_WhenExistingEmailLogsInRepeatedly_ReturnsTokensForSameUser(int loginCount)
    {
        var user = new User { Id = Guid.NewGuid(), Username = "Player" };
        var provider = new StubPasswordAuthProvider(user);
        var tokenService = new StubTokenService();
        using var dbContext = new UserDbContext(new DbContextOptionsBuilder<UserDbContext>().Options);
        var service = new AuthService(new StubAuthProviderSelector(provider), tokenService, dbContext);

        for (var i = 0; i < loginCount; i++)
        {
            var credentials = new PasswordCredentials("player@example.com", "test-password");

            var result = await service.Login(credentials);

            Assert.NotNull(result);
            Assert.Equal("test-access-token", result.AccessToken);
            Assert.Equal("test-refresh-token", result.RefreshToken);
            Assert.Equal(i + 1, provider.LoginCalls.Count);
            Assert.Same(credentials, provider.LoginCalls[i]);
            Assert.Equal(i + 1, tokenService.AccessTokenCalls.Count);
            Assert.Equal(i + 1, tokenService.RefreshTokenCalls.Count);
            Assert.Same(user, tokenService.AccessTokenCalls[i]);
            Assert.Same(user, tokenService.RefreshTokenCalls[i]);
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Login_WhenCredentialsAreInvalid_ReturnsNullWithoutGeneratingTokens(bool loginSuccessfullyFirst)
    {
        var user = new User { Id = Guid.NewGuid(), Username = "Player" };
        var provider = new StubPasswordAuthProvider(user);
        var tokenService = new StubTokenService();
        using var dbContext = new UserDbContext(new DbContextOptionsBuilder<UserDbContext>().Options);
        var service = new AuthService(new StubAuthProviderSelector(provider), tokenService, dbContext);

        if (loginSuccessfullyFirst)
        {
            var firstResult = await service.Login(new PasswordCredentials("player@example.com", "test-password"));
            Assert.NotNull(firstResult);
        }

        provider.LoginResult = null;
        var credentials = new PasswordCredentials("player@example.com", "wrong-password");

        var result = await service.Login(credentials);

        Assert.Null(result);
        var successfulLoginCount = loginSuccessfullyFirst ? 1 : 0;
        Assert.Equal(successfulLoginCount + 1, provider.LoginCalls.Count);
        Assert.Same(credentials, provider.LoginCalls[^1]);
        Assert.Equal(successfulLoginCount, tokenService.AccessTokenCalls.Count);
        Assert.Equal(successfulLoginCount, tokenService.RefreshTokenCalls.Count);
    }

    [Fact]
    public async Task Register_WhenProviderAcceptsCredentials_ReturnsRegisteredUser()
    {
        var credentials = new PasswordCredentials("new-player@example.com", "test-password");
        var user = new User { Id = Guid.NewGuid(), Username = "Player" };
        var provider = new StubPasswordAuthProvider(null) { RegisterResult = user };
        var tokenService = new StubTokenService();
        using var dbContext = new UserDbContext(new DbContextOptionsBuilder<UserDbContext>().Options);
        var service = new AuthService(new StubAuthProviderSelector(provider), tokenService, dbContext);

        var result = await service.Register(credentials);

        Assert.Same(user, result);
        Assert.Same(credentials, Assert.Single(provider.RegisterCalls));
        Assert.Empty(provider.LoginCalls);
        Assert.Empty(tokenService.AccessTokenCalls);
        Assert.Empty(tokenService.RefreshTokenCalls);
    }

    [Fact]
    public async Task Register_WhenProviderRejectsExistingEmail_ReturnsNull()
    {
        var credentials = new PasswordCredentials("existing-player@example.com", "test-password");
        // Занятую почту проверяет провайдер: сервис получает от него null.
        var provider = new StubPasswordAuthProvider(null) { RegisterResult = null };
        var tokenService = new StubTokenService();
        using var dbContext = new UserDbContext(new DbContextOptionsBuilder<UserDbContext>().Options);
        var service = new AuthService(new StubAuthProviderSelector(provider), tokenService, dbContext);

        var result = await service.Register(credentials);

        Assert.Null(result);
        Assert.Same(credentials, Assert.Single(provider.RegisterCalls));
        Assert.Empty(provider.LoginCalls);
        Assert.Empty(tokenService.AccessTokenCalls);
        Assert.Empty(tokenService.RefreshTokenCalls);
    }

    private sealed class StubPasswordAuthProvider(User? user) : IAuthProvider<PasswordCredentials>
    {
        public User? LoginResult { get; set; } = user;
        public User? RegisterResult { get; init; }
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

    private sealed class StubAuthProviderSelector(IAuthProvider<PasswordCredentials> provider)
        : IAuthProviderSelector
    {
        public IAuthProvider<TCredentials> GetAuthProvider<TCredentials>()
            where TCredentials : AuthCredentials
        {
            return provider as IAuthProvider<TCredentials>
                ?? throw new NotSupportedException("Only password credentials are expected in this test.");
        }
    }

    private sealed class StubTokenService : ITokenService
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

        public Task<string> GenerateAccesToken(string refreshToken) =>
            throw new NotSupportedException("Token refresh is not expected in this test.");
    }
}
