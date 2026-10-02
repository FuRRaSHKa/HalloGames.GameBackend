using GameBackend.HalloGames.Time;
using GameBackend.HalloGames.Tokens;
using GameBackend.HalloGames.Tokens.Data;
using GameBackend.HalloGames.Users.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;

namespace GameBackend.Tests.Contexts;

internal sealed class TokenTestContext : IDisposable
{
    public const string SigningKeyId = "test-key";

    public RSAParameters PublicKey { get; }

    public FakeTimeService Time { get; } = new()
    {
        UtcNow = new DateTime(2020, 1, 1, 12, 0, 0, DateTimeKind.Utc)
    };

    public RefreshTokenDbContext RefreshDb { get; } = new(
        new DbContextOptionsBuilder<RefreshTokenDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    public UserDbContext UserDb { get; } = new(
        new DbContextOptionsBuilder<UserDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    public TokenService Service { get; }

    public TokenTestContext()
    {
        using var rsa = RSA.Create(2048);
        PublicKey = rsa.ExportParameters(false);

        var configuration = new ConfigurationBuilder().AddInMemoryCollection(
            new Dictionary<string, string?>
            {
                ["Jwt:PrivateKeyPem"] = rsa.ExportPkcs8PrivateKeyPem(),
                ["Jwt:KeyId"] = SigningKeyId,
                ["Jwt:Issuer"] = "tests",
                ["Jwt:Audience"] = "tests",
                ["Jwt:AccessTokenExpirationMinutes"] = "15",
                ["Jwt:RefreshTokenExpirationDays"] = "7"
            }).Build();

        Service = new TokenService(configuration, RefreshDb, UserDb, Time);
    }

    public void Dispose()
    {
        RefreshDb.Dispose();
        UserDb.Dispose();
    }

    internal sealed class FakeTimeService : ITimeService
    {
        public DateTime UtcNow { get; set; }
    }
}
