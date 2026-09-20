using GameBackend.HalloGames.Auth.Api.AuthProviders;
using GameBackend.HalloGames.Auth.Api.Models.Credentials;
using GameBackend.HalloGames.Auth.Api.Models.Identities;
using GameBackend.HalloGames.Tokens.Models;
using Microsoft.EntityFrameworkCore;

namespace GameBackend.HalloGames.Auth.Api.Data
{
    public class PasswordDbContext : DbContext
    {
        public DbSet<PasswordUserIdentity> Passwords { get; set; }

        public PasswordDbContext(DbContextOptions<PasswordDbContext> options) : base(options)
        {

        }
    }
}
