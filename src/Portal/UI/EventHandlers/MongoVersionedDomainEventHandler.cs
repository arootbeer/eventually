using System;
using Eventually.Interfaces.DomainEvents;
using Eventually.Portal.UI.ViewModel;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace Eventually.Portal.UI.EventHandlers
{
    //TODO: Pull this class and its base classes out into a more generic library
    public abstract class MongoVersionedDomainEventHandler<TEvent, TViewModel> : MongoDomainEventHandler<TEvent, TViewModel>
        where TEvent : class, IDomainEvent
        where TViewModel : class, IViewModel
    {
        protected MongoVersionedDomainEventHandler(IMongoDatabase mongo, ILoggerFactory loggerFactory)
            : base(mongo, loggerFactory) { }

        protected TViewModel GetEntityFor(TEvent domainEvent)
        {
            return Collection.Find(vm => vm.Id == domainEvent.EntityId).SingleOrDefault();
        }

        public override void Handle(TEvent domainEvent)
        {
            var entity = GetEntityFor(domainEvent);

            if (domainEvent is IEntityCreatedEvent && entity is null)
            {
                HandleInternal(domainEvent);
                return;
            }

            if (entity is null || entity.Version != domainEvent.EntityVersion - 1)
            {
                throw new NotImplementedException(
                    "Need to implement 'bring up to date' functionality in DomainEventHandlerBase"
                );
            }

            if (entity.Version >= domainEvent.EntityVersion)
            {
                return;
            }

            HandleInternal(domainEvent);
        }

        protected FilterDefinition<TViewModel> BuildVersionedFilter(TEvent domainEvent)
        {
            return Filter.And(
                Filter.Eq(vm => vm.Id, domainEvent.EntityId),
                Filter.Eq(vm => vm.Version, domainEvent.EntityVersion - 1)
            );
        }

        protected UpdateDefinition<TViewModel> BuildVersionUpdate(TEvent domainEvent)
        {
            return Update.Set(vm => vm.Version, domainEvent.EntityVersion);
        }
    }
}