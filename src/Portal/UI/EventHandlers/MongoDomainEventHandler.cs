using Eventually.Domain.EventHandling;
using Eventually.Interfaces.DomainEvents;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace Eventually.Portal.UI.EventHandlers
{
    
    public abstract class MongoDomainEventHandler<TEvent, TCollection> : MongoDomainEventHandler<TEvent>
        where TEvent : class, IDomainEvent
        where TCollection : class
    {
        protected readonly IMongoCollection<TCollection> Collection;
        
        protected MongoDomainEventHandler(IMongoDatabase mongo, ILoggerFactory loggerFactory) : base(mongo, loggerFactory)
        {
            Collection = GetCollection<TCollection>();
        }

        protected FilterDefinitionBuilder<TCollection> Filter => Builders<TCollection>.Filter;
        
        protected UpdateDefinitionBuilder<TCollection> Update => Builders<TCollection>.Update;
    }
    
    public abstract class MongoDomainEventHandler<TEvent> : DomainEventHandlerBase<TEvent> where TEvent : class, IDomainEvent
    {
        private readonly IMongoDatabase _mongo;

        protected MongoDomainEventHandler(IMongoDatabase mongo, ILoggerFactory loggerFactory) : base(loggerFactory)
        {
            _mongo = mongo;
        }
        
        protected IMongoCollection<TCollection> GetCollection<TCollection>()
        {
            return _mongo.GetCollection<TCollection>(typeof(TCollection).Name);
        }
    }
}