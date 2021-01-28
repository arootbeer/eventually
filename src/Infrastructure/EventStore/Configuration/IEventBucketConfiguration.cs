namespace Eventually.Infrastructure.EventStore.Configuration
{
    public interface IEventBucketConfiguration
    {
        string Id { get; }

        long Checkpoint { get; }
    }
}