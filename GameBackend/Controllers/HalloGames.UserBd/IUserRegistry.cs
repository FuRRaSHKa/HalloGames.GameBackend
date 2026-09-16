namespace GameBackend.Controllers.HalloGames.UserBd
{
    public interface IUserRegistry
    {
        public void Register(string email, string password);
    }
}
