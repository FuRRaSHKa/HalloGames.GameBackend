using GameBackend.HalloGames.Auth.Api.AuthProviders;
using GameBackend.HalloGames.Auth.Api.Models.Credentials;
using GameBackend.HalloGames.Auth.Api.Models.Identities;
using GameBackend.HalloGames.Tokens.Models;
using GameBackend.HalloGames.Users.Models;
using Microsoft.EntityFrameworkCore;

namespace GameBackend.HalloGames.Auth.Api.Data
{
    public class PasswordDbContext : DbContext
    {
        public DbSet<PasswordUserIdentity> Passwords { get; set; }
        public DbSet<User> Users { get; set; }

        public PasswordDbContext(DbContextOptions<PasswordDbContext> options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var passwords = modelBuilder.Entity<PasswordUserIdentity>();

            passwords.HasIndex(x => x.ExternalId)
                .IsUnique()
                .HasDatabaseName("UX_Passwords_Email");

            passwords.HasOne<User>()
                .WithOne()
                .HasForeignKey<PasswordUserIdentity>(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
