using Eventually.Infrastructure.EventStore;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

namespace Eventually.Portal.UI.ViewModel
{
    public class MongoLastCommitCheckpointProvider : ILastCommitCheckpointProvider
    {
        private readonly IMongoCollection<Checkpoint> _checkpoints;

        public MongoLastCommitCheckpointProvider(IMongoDatabase database)
        {
            _checkpoints = database.GetCollection<Checkpoint>("EventStoreCommitCheckpoints");
        }

        public long GetLastCheckpointToken(string bucketId)
        {
            bucketId ??= "default";
            return _checkpoints
                       .Find(c => c.Id == bucketId)
                       .SingleOrDefault()
                       ?.Token
                   ?? 0;
        }

        public async void SetLastCheckpointToken(string bucketId, long newToken)
        {
            var checkpoint = new Checkpoint {Id = bucketId, Token = newToken};
            await _checkpoints.ReplaceOneAsync(
                c => c.Id == bucketId,
                checkpoint,
                new ReplaceOptions {IsUpsert = true}
            );
        }

        public class Checkpoint
        {
            [BsonRepresentation(BsonType.String)]
            [BsonId]
            public string Id { get; set; }

            public long Token { get; set; }
        }
    }
}