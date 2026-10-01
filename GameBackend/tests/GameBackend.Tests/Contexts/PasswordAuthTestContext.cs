using GameBackend.HalloGames.Auth.Api.AuthProviders;
using GameBackend.HalloGames.Auth.Api.Controllers;
using GameBackend.HalloGames.Auth.Api.Data;
using GameBackend.HalloGames.Auth.Api.Models.Credentials;
using GameBackend.HalloGames.Auth.Api.Models.Identities;
using GameBackend.HalloGames.Auth.Api.Services;
using GameBackend.HalloGames.Tokens;
using GameBackend.HalloGames.Users.Data;
using GameBackend.HalloGames.Users.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GameBackend.Tests.Contexts;

internal sealed class PasswordAuthTestContext : IDisposable
{
    private readonly ServiceProvider _services;
    private readonly UserDbContext _userDb;
    private readonly bool _ownsUserDb;

    public PasswordDbContext PasswordDb { get; } = new(
        new DbContextOptionsBuilder<PasswordDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    public IAuthProvider<PasswordCredentials> Provider { get; }
    public AuthService Service { get; }
    public AuthController Controller { get; }

    public PasswordAuthTestContext(ITokenService tokenService, UserDbContext? userDb = null)
    {
        _ownsUserDb = userDb is null;
        _userDb = userDb ?? new UserDbContext(new DbContextOptionsBuilder<UserDbContext>().Options);
        Provider = new PasswordAuthProvider(PasswordDb);
        _services = new ServiceCollection()
            .AddSingleton(Provider)
            .BuildServiceProvider();

        Service = new AuthService(new AuthProviderSelector(_services), tokenService, _userDb);
        Controller = new AuthController(Service);
    }

    public async Task<User> CreateUserAsync(string email, string password)
    {
        var user = new User { Id = Guid.NewGuid(), Username = "Player" };
        PasswordDb.Users.Add(user);
        PasswordDb.Passwords.Add(new PasswordUserIdentity
        {
            UserId = user.Id,
            ExternalId = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password)
        });

        await PasswordDb.SaveChangesAsync();
        PasswordDb.ChangeTracker.Clear();
        return user;
    }

    public void Dispose()
    {
        _services.Dispose();
        PasswordDb.Dispose();
        if (_ownsUserDb)
        {
            _userDb.Dispose();
        }
    }
}
