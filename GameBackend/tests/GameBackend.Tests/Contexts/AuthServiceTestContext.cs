using GameBackend.HalloGames.Auth.Api.Services;
using GameBackend.HalloGames.Users.Data;
using GameBackend.HalloGames.Users.Models;
using Microsoft.EntityFrameworkCore;

namespace GameBackend.Tests.Contexts;

internal sealed class AuthServiceTestContext : IDisposable
{
    private readonly UserDbContext _userDb = new(new DbContextOptionsBuilder<UserDbContext>().Options);

    public User User { get; } = new() { Id = Guid.NewGuid(), Username = "Player" };
    public StubPasswordAuthProvider Provider { get; }
    public StubTokenService Tokens { get; } = new();
    public AuthService Service { get; }

    public AuthServiceTestContext()
    {
        Provider = new StubPasswordAuthProvider(User);
        Service = new AuthService(new StubAuthProviderSelector(Provider), Tokens, _userDb);
    }

    public void Dispose() => _userDb.Dispose();
}
