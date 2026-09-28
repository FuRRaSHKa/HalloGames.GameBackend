using GameBackend.HalloGames.Auth.Api.AuthProviders;
using GameBackend.HalloGames.Auth.Api.Controllers;
using GameBackend.HalloGames.Auth.Api.Data;
using GameBackend.HalloGames.Auth.Api.Models.Credentials;
using GameBackend.HalloGames.Auth.Api.Models.Identities;
using GameBackend.HalloGames.Auth.Api.Models.Requests;
using GameBackend.HalloGames.Auth.Api.Models.Results;
using GameBackend.HalloGames.Auth.Api.Services;
using GameBackend.HalloGames.Users.Models;
using GameBackend.HalloGames.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.IdentityModel.Tokens.Jwt;

namespace GameBackend.Tests;

public class AuthTokenFlowTests
{
    [Fact]
    public async Task LoginByEmail_WhenCredentialsAreValid_GeneratesTokensForAuthenticatedUser()
    {
        const string email = "player@example.com";
        const string password = "test-password";
        using var context = new TokenTestContext();
        using var passwordDb = new PasswordDbContext(
            new DbContextOptionsBuilder<PasswordDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

        var user = new User { Id = Guid.NewGuid(), Username = "Player" };
        passwordDb.Users.Add(user);
        passwordDb.Passwords.Add(new PasswordUserIdentity
        {
            UserId = user.Id,
            ExternalId = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password)
        });

        await passwordDb.SaveChangesAsync();
        passwordDb.ChangeTracker.Clear();
        using var services = new ServiceCollection()
            .AddSingleton<IAuthProvider<PasswordCredentials>>(new PasswordAuthProvider(passwordDb))
            .BuildServiceProvider();

        var authService = new AuthService(
            new AuthProviderSelector(services), context.Service, context.UserDb);

        var controller = new AuthController(authService);

        var result = await controller.LoginByEmail(new EmailLoginRequest(email, password));

        var tokens = Assert.IsType<TokenResult>(Assert.IsType<OkObjectResult>(result).Value);
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(tokens.AccessToken);
        Assert.Equal(user.Id.ToString(), jwt.Subject);
        Assert.Equal(context.Time.UtcNow.AddMinutes(15), jwt.ValidTo);
        Assert.False(string.IsNullOrWhiteSpace(tokens.RefreshToken));
        context.RefreshDb.ChangeTracker.Clear();
        var storedToken = await context.RefreshDb.RefreshTokens.SingleAsync();
        Assert.Equal(user.Id, storedToken.UserId);
        Assert.Equal(HashUtils.HashRefrestToken(tokens.RefreshToken), storedToken.Token);
        Assert.Equal(context.Time.UtcNow.AddDays(7), storedToken.Expires);
    }
}
