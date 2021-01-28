namespace Eventually.Infrastructure.EventStore
{
    public interface ILastCommitCheckpointProvider
    {
        long GetLastCheckpointToken(string bucketId);

        void SetLastCheckpointToken(string bucketId, long newToken);
    }
}