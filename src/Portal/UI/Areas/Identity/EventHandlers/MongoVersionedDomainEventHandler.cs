using Eventually.Interfaces.DomainEvents;
using Eventually.Portal.UI.ViewModel;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace Eventually.Portal.UI.Areas.Identity.EventHandlers
{
    public abstract class MongoVersionedDomainEventHandler<TEvent, TViewModel> : MongoDomainEventHandler<TEvent, TViewModel>
        where TEvent : class, IDomainEvent
        where TViewModel : class, IViewModel
    {
        protected MongoVersionedDomainEventHandler(IMongoDatabase mongo, ILoggerFactory loggerFactory) : base(mongo, loggerFactory) { }

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