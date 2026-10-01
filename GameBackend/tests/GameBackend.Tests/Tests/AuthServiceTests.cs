using GameBackend.HalloGames.Auth.Api.Models.Credentials;
using GameBackend.Tests.Contexts;

namespace GameBackend.Tests;

public class AuthServiceTests
{
    [Fact]
    public async Task Login_WhenPasswordIsIncorrect_ReturnsNullWithoutGeneratingTokens()
    {
        const string email = "player@example.com";
        const string correctPassword = "correct-password";
        var tokenService = new StubTokenService();
        using var context = new PasswordAuthTestContext(tokenService);
        var user = await context.CreateUserAsync(email, correctPassword);
        var provider = context.Provider;
        // Проверяем, что учетная запись доступна и правильный пароль принимается.
        var authenticatedUser = await provider.Login(new PasswordCredentials(email, correctPassword));
        Assert.NotNull(authenticatedUser);
        Assert.Equal(user.Id, authenticatedUser.Id);

        var service = context.Service;

        var result = await service.Login(new PasswordCredentials(email, "wrong-password"));

        Assert.Null(result);
        Assert.Empty(tokenService.AccessTokenCalls);
        Assert.Empty(tokenService.RefreshTokenCalls);
    }

    [Fact]
    public async Task Login_WhenCredentialsAreValid_ReturnsTokensForAuthenticatedUser()
    {
        var credentials = new PasswordCredentials("player@example.com", "test-password");
        using var context = new AuthServiceTestContext();

        var result = await context.Service.Login(credentials);

        Assert.NotNull(result);
        Assert.Equal("test-access-token", result.AccessToken);
        Assert.Equal("test-refresh-token", result.RefreshToken);
        Assert.Same(credentials, Assert.Single(context.Provider.LoginCalls));
        Assert.Same(context.User, Assert.Single(context.Tokens.AccessTokenCalls));
        Assert.Same(context.User, Assert.Single(context.Tokens.RefreshTokenCalls));
    }

    [Theory]
    [InlineData(2)]
    [InlineData(3)]
    public async Task Login_WhenExistingEmailLogsInRepeatedly_ReturnsTokensForSameUser(int loginCount)
    {
        using var context = new AuthServiceTestContext();

        for (var i = 0; i < loginCount; i++)
        {
            var credentials = new PasswordCredentials("player@example.com", "test-password");

            var result = await context.Service.Login(credentials);

            Assert.NotNull(result);
            Assert.Equal("test-access-token", result.AccessToken);
            Assert.Equal("test-refresh-token", result.RefreshToken);
            Assert.Equal(i + 1, context.Provider.LoginCalls.Count);
            Assert.Same(credentials, context.Provider.LoginCalls[i]);
            Assert.Equal(i + 1, context.Tokens.AccessTokenCalls.Count);
            Assert.Equal(i + 1, context.Tokens.RefreshTokenCalls.Count);
            Assert.Same(context.User, context.Tokens.AccessTokenCalls[i]);
            Assert.Same(context.User, context.Tokens.RefreshTokenCalls[i]);
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Login_WhenCredentialsAreInvalid_ReturnsNullWithoutGeneratingTokens(bool loginSuccessfullyFirst)
    {
        using var context = new AuthServiceTestContext();

        if (loginSuccessfullyFirst)
        {
            var firstResult = await context.Service.Login(new PasswordCredentials("player@example.com", "test-password"));
            Assert.NotNull(firstResult);
        }

        context.Provider.LoginResult = null;
        var credentials = new PasswordCredentials("player@example.com", "wrong-password");

        var result = await context.Service.Login(credentials);

        Assert.Null(result);
        var successfulLoginCount = loginSuccessfullyFirst ? 1 : 0;
        Assert.Equal(successfulLoginCount + 1, context.Provider.LoginCalls.Count);
        Assert.Same(credentials, context.Provider.LoginCalls[^1]);
        Assert.Equal(successfulLoginCount, context.Tokens.AccessTokenCalls.Count);
        Assert.Equal(successfulLoginCount, context.Tokens.RefreshTokenCalls.Count);
    }

    [Fact]
    public async Task Register_WhenProviderAcceptsCredentials_ReturnsRegisteredUser()
    {
        var credentials = new PasswordCredentials("new-player@example.com", "test-password");
        using var context = new AuthServiceTestContext();
        context.Provider.LoginResult = null;
        context.Provider.RegisterResult = context.User;

        var result = await context.Service.Register(credentials);

        Assert.Same(context.User, result);
        Assert.Same(credentials, Assert.Single(context.Provider.RegisterCalls));
        Assert.Empty(context.Provider.LoginCalls);
        Assert.Empty(context.Tokens.AccessTokenCalls);
        Assert.Empty(context.Tokens.RefreshTokenCalls);
    }

    [Fact]
    public async Task Register_WhenProviderRejectsExistingEmail_ReturnsNull()
    {
        var credentials = new PasswordCredentials("existing-player@example.com", "test-password");
        // Занятую почту проверяет провайдер: сервис получает от него null.
        using var context = new AuthServiceTestContext();
        context.Provider.LoginResult = null;

        var result = await context.Service.Register(credentials);

        Assert.Null(result);
        Assert.Same(credentials, Assert.Single(context.Provider.RegisterCalls));
        Assert.Empty(context.Provider.LoginCalls);
        Assert.Empty(context.Tokens.AccessTokenCalls);
        Assert.Empty(context.Tokens.RefreshTokenCalls);
    }
}
