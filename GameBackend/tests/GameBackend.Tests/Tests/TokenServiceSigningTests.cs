using GameBackend.HalloGames.Users.Models;
using GameBackend.Tests.Contexts;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;

namespace GameBackend.Tests;

public class TokenServiceSigningTests
{
    [Fact]
    public async Task GenerateAccessToken_UsesRs256AndConfiguredKeyId()
    {
        using var context = new TokenTestContext();
        var user = new User { Id = Guid.NewGuid(), Username = "Player" };

        var token = await context.Service.GenerateAccesToken(user);

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
        Assert.Equal(SecurityAlgorithms.RsaSha256, jwt.Header.Alg);
        Assert.Equal(TokenTestContext.SigningKeyId, jwt.Header.Kid);
    }

    [Fact]
    public async Task GenerateAccessToken_SignatureIsValidWithMatchingPublicKey()
    {
        using var context = new TokenTestContext();
        var user = new User { Id = Guid.NewGuid(), Username = "Player" };

        var token = await context.Service.GenerateAccesToken(user);

        var handler = new JwtSecurityTokenHandler();
        handler.ValidateToken(token, CreateValidationParameters(context.PublicKey), out var validatedToken);

        var jwt = Assert.IsType<JwtSecurityToken>(validatedToken);
        Assert.Equal(user.Id.ToString(), jwt.Subject);
    }

    [Fact]
    public async Task GenerateAccessToken_SignatureIsRejectedWithDifferentPublicKey()
    {
        using var context = new TokenTestContext();
        using var otherRsa = RSA.Create(2048);
        var user = new User { Id = Guid.NewGuid(), Username = "Player" };

        var token = await context.Service.GenerateAccesToken(user);

        var validationParameters = CreateValidationParameters(otherRsa.ExportParameters(false));
        var handler = new JwtSecurityTokenHandler();

        Assert.Throws<SecurityTokenInvalidSignatureException>(() =>
            handler.ValidateToken(token, validationParameters, out _));
    }

    private static TokenValidationParameters CreateValidationParameters(RSAParameters publicKey)
    {
        return new TokenValidationParameters
        {
            IssuerSigningKey = new RsaSecurityKey(publicKey)
            {
                KeyId = TokenTestContext.SigningKeyId
            },

            ValidateIssuerSigningKey = true,
            RequireSignedTokens = true,
            ValidAlgorithms = new[] { SecurityAlgorithms.RsaSha256 },
            ValidIssuer = "tests",
            ValidAudience = "tests",
            // Expiration is checked separately against the injected test time.
            ValidateLifetime = false
        };
    }
}
