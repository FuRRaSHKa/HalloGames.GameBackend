using GameBackend.HalloGames.Users.Models;

namespace GameBackend.HalloGames.Users
{
    public interface IUserRepository
    {
        public Task Register(User user);
    }
}
