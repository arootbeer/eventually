using Eventually.Portal.Domain.Counter.Events;
using Eventually.Portal.UI.Areas.Counter.Data;
using Eventually.Portal.UI.Areas.Identity.Data;
using Eventually.Portal.UI.EventHandlers;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace Eventually.Portal.UI.Areas.Counter.EventHandling
{
    public class GlobalCounterCreatedEventHandler : PortalUIEventHandler<GlobalCounterCreated, GlobalCounterState>
    {
        public GlobalCounterCreatedEventHandler(
            IUserStore<PortalUser> userStore,
            IMongoDatabase database,
            ILoggerFactory loggerFactory
        ) : base(userStore, database, loggerFactory)
        {
        }

        protected override void HandleInternal(GlobalCounterCreated domainEvent)
        {
            var userTask = GetUserFor(domainEvent).ConfigureAwait(false).GetAwaiter();
            var state = new GlobalCounterState
            {
                Id = domainEvent.EntityId,
                RecentHistory =
                {
                    new GlobalCounterStateChange
                    {
                        ChangeSequence = domainEvent.EntityVersion,
                        Timestamp = domainEvent.Timestamp,
                        UserName = userTask.GetResult().UserName
                    }
                },
                Version = domainEvent.EntityVersion,
                Value = 0
            };

            Collection.InsertOne(state);
        }
    }
}