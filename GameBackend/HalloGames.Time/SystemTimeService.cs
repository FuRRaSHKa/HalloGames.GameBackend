namespace GameBackend.HalloGames.Time
{
    public sealed class SystemTimeService : ITimeService
    {
        public DateTime UtcNow => DateTime.UtcNow;
    }
}
