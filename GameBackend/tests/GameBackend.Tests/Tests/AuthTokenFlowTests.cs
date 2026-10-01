using GameBackend.HalloGames.Auth.Api.Models.Requests;
using GameBackend.HalloGames.Auth.Api.Models.Results;
using GameBackend.HalloGames.Utils;
using GameBackend.Tests.Contexts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
        using var authContext = new PasswordAuthTestContext(context.Service, context.UserDb);
        var user = await authContext.CreateUserAsync(email, password);

        var result = await authContext.Controller.LoginByEmail(new EmailLoginRequest(email, password));

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
