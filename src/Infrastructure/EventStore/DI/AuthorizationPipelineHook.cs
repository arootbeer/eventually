using NEventStore;

namespace Eventually.Infrastructure.EventStore.DI
{
    internal class AuthorizationPipelineHook : IPipelineHook
    {
        public void Dispose()
        {

        }

        public ICommit Select(ICommit committed)
        {
            return committed;
        }

        public bool PreCommit(CommitAttempt attempt)
        {
            return true;
        }

        public void PostCommit(ICommit committed)
        {

        }

        public void OnPurge(string bucketId)
        {

        }

        public void OnDeleteStream(string bucketId, string streamId)
        {

        }
    }
}