using GameBackend.HalloGames.Tokens.Models;
using GameBackend.HalloGames.Users.Models;
using GameBackend.HalloGames.Utils;
using GameBackend.Tests.Contexts;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;

namespace GameBackend.Tests;

public class TokenServiceTimeTests
{
    [Fact]
    public async Task GenerateAccessToken_UsesInjectedTimeForExpiration()
    {
        using var context = new TokenTestContext();
        var user = new User { Id = Guid.NewGuid(), Username = "Player" };

        var token = await context.Service.GenerateAccesToken(user);

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
        Assert.Equal(context.Time.UtcNow.AddMinutes(15), jwt.ValidTo);
        Assert.Equal(user.Id.ToString(), jwt.Subject);
    }

    [Fact]
    public async Task GenerateRefreshToken_StoresTokenWithExpirationFromInjectedTime()
    {
        using var context = new TokenTestContext();
        var user = new User { Id = Guid.NewGuid(), Username = "Player" };

        var token = await context.Service.GenerateRefreshToken(user);

        context.RefreshDb.ChangeTracker.Clear();
        var storedToken = await context.RefreshDb.RefreshTokens.SingleAsync();
        Assert.Equal(context.Time.UtcNow.AddDays(7), storedToken.Expires);
        Assert.Equal(user.Id, storedToken.UserId);
        Assert.Equal(HashUtils.HashRefrestToken(token), storedToken.Token);
    }

    [Fact]
    public async Task GenerateAccessToken_BeforeRefreshTokenExpires_UsesCurrentInjectedTime()
    {
        using var context = new TokenTestContext();
        var expires = context.Time.UtcNow.AddDays(7);
        var user = await StoreRefreshToken(context, expires);
        context.Time.UtcNow = expires.AddSeconds(-1);

        var token = await context.Service.GenerateAccesToken("refresh-token");

        Assert.NotNull(token);
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
        Assert.Equal(context.Time.UtcNow.AddMinutes(15), jwt.ValidTo);
        Assert.Equal(user.Id.ToString(), jwt.Subject);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    public async Task GenerateAccessToken_AtOrAfterRefreshTokenExpiration_ReturnsNull(long ticksAfterExpiration)
    {
        using var context = new TokenTestContext();
        var expires = context.Time.UtcNow.AddDays(7);
        await StoreRefreshToken(context, expires);
        context.Time.UtcNow = expires.AddTicks(ticksAfterExpiration);

        var token = await context.Service.GenerateAccesToken("refresh-token");

        Assert.Null(token);
    }

    private static async Task<User> StoreRefreshToken(TokenTestContext context, DateTime expires)
    {
        var user = new User { Id = Guid.NewGuid(), Username = "Player" };
        context.UserDb.Users.Add(user);
        await context.UserDb.SaveChangesAsync();
        context.RefreshDb.RefreshTokens.Add(new RefreshToken
        {
            UserId = user.Id,
            Token = HashUtils.HashRefrestToken("refresh-token"),
            Expires = expires
        });

        await context.RefreshDb.SaveChangesAsync();
        context.UserDb.ChangeTracker.Clear();
        context.RefreshDb.ChangeTracker.Clear();
        return user;
    }
}
