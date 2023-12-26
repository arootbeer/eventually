using System.Collections.Generic;
using System.Linq;
using Eventually.Portal.Domain.Counter.Events;
using Eventually.Portal.UI.Areas.Counter.Data;
using Eventually.Portal.UI.Areas.Identity.Data;
using Eventually.Portal.UI.EventHandlers;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace Eventually.Portal.UI.Areas.Counter.EventHandling
{
    public abstract class GlobalCounterChangedEventHandler<TEvent> : PortalUIEventHandler<TEvent, GlobalCounterState>
        where TEvent : class, IGlobalCounterChangedEvent
    {
        protected GlobalCounterChangedEventHandler(
            IUserStore<PortalUser> userStore,
            IMongoDatabase database,
            ILoggerFactory loggerFactory
        ) : base(userStore, database, loggerFactory)
        {
        }

        protected virtual List<GlobalCounterStateChange> UpdateGlobalCounterHistory(TEvent domainEvent)
        {
            var userTask = GetUserFor(domainEvent).ConfigureAwait(false).GetAwaiter();

            var state = GetEntityFor(domainEvent);
            var historySize = state.RecentHistory.Count;
            var updatedHistory = historySize < 5 ? state.RecentHistory : state.RecentHistory.Skip(1).Take(4).ToList();
            updatedHistory.Add(
                new GlobalCounterStateChange
                {
                    ChangeSequence = domainEvent.EntityVersion,
                    Timestamp = domainEvent.Timestamp,
                    PreviousValue = domainEvent.PreviousValue,
                    UserName = userTask.GetResult().UserName
                }
            );
            return updatedHistory;
        }
    }
}