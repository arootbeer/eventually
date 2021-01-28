namespace Eventually.Infrastructure.EventStore.Configuration
{
    public interface IDatabaseConnectionConfiguration
    {
        string ConnectionString { get; }
    }
}