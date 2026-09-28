using GameBackend.HalloGames.Auth.Api.AuthProviders;
using GameBackend.HalloGames.Auth.Api.Controllers;
using GameBackend.HalloGames.Auth.Api.Data;
using GameBackend.HalloGames.Auth.Api.Models.Credentials;
using GameBackend.HalloGames.Auth.Api.Models.Requests;
using GameBackend.HalloGames.Auth.Api.Models.Results;
using GameBackend.HalloGames.Auth.Api.Services;
using GameBackend.HalloGames.Tokens;
using GameBackend.HalloGames.Users.Data;
using GameBackend.HalloGames.Users.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GameBackend.Tests;

public class PasswordWhitespaceFlowTests
{
    [Theory]
    [InlineData(" pass word")]
    [InlineData("pass word ")]
    [InlineData(" pass word ")]
    public async Task RegisterAndLogin_PreservePasswordWhitespaceThroughoutAuthFlow(string password)
    {
        const string email = "player@example.com";
        using var passwordDb = new PasswordDbContext(
            new DbContextOptionsBuilder<PasswordDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options);
        using var userDb = new UserDbContext(
            new DbContextOptionsBuilder<UserDbContext>().Options);
        var service = new AuthService(
            new PasswordProviderSelector(new PasswordAuthProvider(passwordDb)),
            new StubTokenService(), userDb);
        var controller = new AuthController(service);

        var registration = await controller.RegisterByEmail(new EmailRegisterRequest(email, password));

        var user = Assert.IsType<User>(Assert.IsType<OkObjectResult>(registration).Value);
        passwordDb.ChangeTracker.Clear();
        var identity = await passwordDb.Passwords.SingleAsync();
        Assert.Equal(user.Id, identity.UserId);
        Assert.True(BCrypt.Net.BCrypt.Verify(password, identity.PasswordHash));
        Assert.False(BCrypt.Net.BCrypt.Verify(password.Trim(), identity.PasswordHash));

        var login = await controller.LoginByEmail(new EmailLoginRequest(email, password));

        Assert.IsType<TokenResult>(Assert.IsType<OkObjectResult>(login).Value);

        var trimmedLogin = await controller.LoginByEmail(new EmailLoginRequest(email, password.Trim()));

        Assert.IsType<UnauthorizedResult>(trimmedLogin);
    }

    private sealed class PasswordProviderSelector(IAuthProvider<PasswordCredentials> provider)
        : IAuthProviderSelector
    {
        public IAuthProvider<TCredentials> GetAuthProvider<TCredentials>()
            where TCredentials : AuthCredentials =>
            provider as IAuthProvider<TCredentials>
                ?? throw new NotSupportedException("Only password credentials are expected in this test.");
    }

    private sealed class StubTokenService : ITokenService
    {
        public Task<string> GenerateAccesToken(User user) => Task.FromResult("access-token");
        public Task<string> GenerateRefreshToken(User user) => Task.FromResult("refresh-token");
        public Task<string> GenerateAccesToken(string refreshToken) =>
            throw new NotSupportedException("Token refresh is not expected in this test.");
    }
}
