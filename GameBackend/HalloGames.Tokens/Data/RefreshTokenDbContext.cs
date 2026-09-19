using GameBackend.HalloGames.Tokens.Models;
using Microsoft.EntityFrameworkCore;

namespace GameBackend.HalloGames.Tokens.Data
{
    public class RefreshTokenDbContext : DbContext
    {
        public DbSet<RefreshToken> RefreshTokens { get; set; }  

        public RefreshTokenDbContext(DbContextOptions<RefreshTokenDbContext> options) : base(options)
        {
        }
    }
}
