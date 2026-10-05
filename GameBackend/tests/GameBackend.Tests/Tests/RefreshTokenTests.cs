using GameBackend.Tests.Contexts;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;

namespace GameBackend.Tests;

public class RefreshTokenTests
{
    [Fact]
    public async Task GenerateAccessToken_WithValidRefreshToken_UsesAssociatedUser()
    {
        using var context = new TokenTestContext();
        await context.CreateUserAsync("Other player");
        var user = await context.CreateUserAsync();
        var refreshToken = await context.Service.GenerateRefreshToken(user);

        var accessToken = await context.Service.GenerateAccesToken(refreshToken);

        Assert.NotNull(accessToken);
        Assert.Equal(user.Id.ToString(), new JwtSecurityTokenHandler().ReadJwtToken(accessToken).Subject);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task GenerateAccessToken_WithUnknownOrModifiedToken_ReturnsNull(bool modifyExistingToken)
    {
        using var context = new TokenTestContext();
        var user = await context.CreateUserAsync();
        var refreshToken = await context.Service.GenerateRefreshToken(user);
        var invalidToken = modifyExistingToken
            ? (refreshToken[0] == 'A' ? "B" : "A") + refreshToken[1..]
            : "unknown-refresh-token";

        var accessToken = await context.Service.GenerateAccesToken(invalidToken);

        Assert.Null(accessToken);
    }

    [Fact]
    public async Task GenerateAccessToken_WithRevokedToken_ReturnsNull()
    {
        using var context = new TokenTestContext();
        var user = await context.CreateUserAsync();
        var refreshToken = await context.Service.GenerateRefreshToken(user);
        var storedToken = await context.RefreshDb.RefreshTokens.SingleAsync();
        storedToken.IsRevoked = true;
        await context.RefreshDb.SaveChangesAsync();
        context.RefreshDb.ChangeTracker.Clear();

        var accessToken = await context.Service.GenerateAccesToken(refreshToken);

        Assert.True(storedToken.Expires > context.Time.UtcNow);
        Assert.Null(accessToken);
    }

    [Fact]
    public async Task GenerateAccessToken_WhenUserWasDeleted_ReturnsNull()
    {
        using var context = new TokenTestContext();
        var user = await context.CreateUserAsync();
        var refreshToken = await context.Service.GenerateRefreshToken(user);
        context.UserDb.Users.Remove(user);
        await context.UserDb.SaveChangesAsync();
        context.UserDb.ChangeTracker.Clear();

        var accessToken = await context.Service.GenerateAccesToken(refreshToken);

        Assert.Null(accessToken);
    }

    [Fact]
    public async Task GenerateAccessToken_WithStoredHash_ReturnsNull()
    {
        using var context = new TokenTestContext();
        var user = await context.CreateUserAsync();
        var refreshToken = await context.Service.GenerateRefreshToken(user);
        context.RefreshDb.ChangeTracker.Clear();
        var storedToken = await context.RefreshDb.RefreshTokens.SingleAsync();

        var accessToken = await context.Service.GenerateAccesToken(storedToken.Token);

        Assert.NotEqual(refreshToken, storedToken.Token);
        Assert.Null(accessToken);
    }
}
