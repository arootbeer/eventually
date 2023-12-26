using Eventually.Portal.Domain.Counter.Events;
using Eventually.Portal.UI.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace Eventually.Portal.UI.Areas.Counter.EventHandling
{
    public class GlobalCounterDecrementedEventHandler : GlobalCounterChangedEventHandler<GlobalCounterDecremented>
    {
        public GlobalCounterDecrementedEventHandler(
            IUserStore<PortalUser> userStore,
            IMongoDatabase database,
            ILoggerFactory loggerFactory
        ) : base(userStore, database, loggerFactory)
        {
        }

        protected override void HandleInternal(GlobalCounterDecremented domainEvent)
        {
            Collection.FindOneAndUpdate(
                BuildVersionedFilter(domainEvent),
                BuildVersionUpdate(domainEvent)
                    .Set(gcs => gcs.Value, domainEvent.PreviousValue - 1)
                    .Set(gcs => gcs.RecentHistory, UpdateGlobalCounterHistory(domainEvent))
            );
        }
    }
}