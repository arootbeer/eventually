namespace Eventually.Infrastructure.EventStore.Configuration
{
    public class EventBucketConfiguration : IEventBucketConfiguration
    {
        public string Id { get; set; }

        public long Checkpoint { get; set; }
    }
}