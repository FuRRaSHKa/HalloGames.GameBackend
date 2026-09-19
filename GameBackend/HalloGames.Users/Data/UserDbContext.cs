using GameBackend.HalloGames.Users.Models;
using Microsoft.EntityFrameworkCore;

namespace GameBackend.HalloGames.Users.Data
{
    public class UserDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }  

        public UserDbContext(DbContextOptions<UserDbContext> options) : base(options)
        {

        }   
    }
}
