using GameBackend.Auth.HalloGames.Auth.Api.Models.Requests;
using GameBackend.HalloGames.Auth.Api.Models.Results;
using GameBackend.Tests.Contexts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;

namespace GameBackend.Tests;

public class AuthRefreshTokenFlowTests
{
    [Fact]
    public async Task RefreshToken_WithValidToken_ReturnsSignedAccessTokenAndOriginalRefreshToken()
    {
        using var context = new TokenTestContext();
        using var authContext = new PasswordAuthTestContext(context.Service, context.UserDb);
        var user = await context.CreateUserAsync();
        var originalAccessToken = await context.Service.GenerateAccesToken(user);
        var refreshToken = await context.Service.GenerateRefreshToken(user);
        context.Time.UtcNow = context.Time.UtcNow.AddHours(1);

        var result = await authContext.Controller.RefreshToken(new RefreshTokenRequest
        {
            RefreshToken = refreshToken
        });

        var tokens = Assert.IsType<TokenResult>(Assert.IsType<OkObjectResult>(result).Value);
        Assert.Equal(refreshToken, tokens.RefreshToken);
        Assert.NotEqual(originalAccessToken, tokens.AccessToken);
        var parameters = new TokenValidationParameters
        {
            IssuerSigningKey = new RsaSecurityKey(context.PublicKey),
            ValidateIssuerSigningKey = true,
            RequireSignedTokens = true,
            ValidAlgorithms = [SecurityAlgorithms.RsaSha256],
            ValidIssuer = "tests",
            ValidAudience = "tests",
            // Lifetime is checked below against the injected time.
            ValidateLifetime = false
        };

        new JwtSecurityTokenHandler().ValidateToken(tokens.AccessToken, parameters, out var validatedToken);
        var jwt = Assert.IsType<JwtSecurityToken>(validatedToken);
        Assert.Equal(user.Id.ToString(), jwt.Subject);
        Assert.Equal(context.Time.UtcNow.AddMinutes(15), jwt.ValidTo);
    }

    [Theory]
    [InlineData("unknown")]
    [InlineData("expired")]
    [InlineData("revoked")]
    [InlineData("deleted-user")]
    public async Task RefreshToken_WithInvalidToken_ReturnsUnauthorized(string scenario)
    {
        using var context = new TokenTestContext();
        using var authContext = new PasswordAuthTestContext(context.Service, context.UserDb);
        var user = await context.CreateUserAsync();
        var refreshToken = await context.Service.GenerateRefreshToken(user);
        var storedToken = await context.RefreshDb.RefreshTokens.SingleAsync();

        switch (scenario)
        {
            case "unknown":
                refreshToken = "unknown-refresh-token";
                break;
            case "expired":
                context.Time.UtcNow = storedToken.Expires;
                break;
            case "revoked":
                storedToken.IsRevoked = true;
                await context.RefreshDb.SaveChangesAsync();
                break;
            case "deleted-user":
                context.UserDb.Users.Remove(user);
                await context.UserDb.SaveChangesAsync();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(scenario));
        }

        context.RefreshDb.ChangeTracker.Clear();
        context.UserDb.ChangeTracker.Clear();

        var result = await authContext.Controller.RefreshToken(new RefreshTokenRequest
        {
            RefreshToken = refreshToken
        });

        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public async Task RefreshToken_WhenReused_ReturnsDistinctAccessTokensWithoutExtendingRefreshExpiration()
    {
        using var context = new TokenTestContext();
        using var authContext = new PasswordAuthTestContext(context.Service, context.UserDb);
        var user = await context.CreateUserAsync();
        var refreshToken = await context.Service.GenerateRefreshToken(user);
        var originalToken = await context.RefreshDb.RefreshTokens.AsNoTracking().SingleAsync();
        var request = new RefreshTokenRequest { RefreshToken = refreshToken };

        var firstResult = await authContext.Controller.RefreshToken(request);
        context.Time.UtcNow = context.Time.UtcNow.AddHours(1);
        var secondResult = await authContext.Controller.RefreshToken(request);

        var firstTokens = Assert.IsType<TokenResult>(Assert.IsType<OkObjectResult>(firstResult).Value);
        var secondTokens = Assert.IsType<TokenResult>(Assert.IsType<OkObjectResult>(secondResult).Value);
        Assert.NotEqual(firstTokens.AccessToken, secondTokens.AccessToken);
        var handler = new JwtSecurityTokenHandler();
        Assert.NotEqual(handler.ReadJwtToken(firstTokens.AccessToken).Id, handler.ReadJwtToken(secondTokens.AccessToken).Id);
        Assert.Equal(refreshToken, firstTokens.RefreshToken);
        Assert.Equal(refreshToken, secondTokens.RefreshToken);
        context.RefreshDb.ChangeTracker.Clear();
        var storedToken = await context.RefreshDb.RefreshTokens.SingleAsync();
        Assert.Equal(originalToken.Id, storedToken.Id);
        Assert.Equal(originalToken.Token, storedToken.Token);
        Assert.Equal(originalToken.UserId, storedToken.UserId);
        Assert.Equal(originalToken.Expires, storedToken.Expires);
        Assert.False(storedToken.IsRevoked);
    }
}
