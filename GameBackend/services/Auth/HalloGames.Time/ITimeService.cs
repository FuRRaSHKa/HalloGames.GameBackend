namespace GameBackend.HalloGames.Time
{
    public interface ITimeService
    {
        DateTime UtcNow { get; }
    }
}
