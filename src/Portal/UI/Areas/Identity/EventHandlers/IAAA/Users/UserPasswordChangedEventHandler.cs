using Eventually.Portal.Domain.IAAA.Events.Users;
using Eventually.Portal.UI.Areas.Identity.Data;
using Eventually.Portal.UI.EventHandlers;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace Eventually.Portal.UI.Areas.Identity.EventHandlers.IAAA.Users
{
    public class UserPasswordChangedEventHandler : MongoVersionedDomainEventHandler<UserPasswordChanged, PortalUser>
    {
        public UserPasswordChangedEventHandler(IMongoDatabase database, ILoggerFactory loggerFactory) : base(database, loggerFactory) { }

        protected override void HandleInternal(UserPasswordChanged domainEvent)
        {
            Collection.FindOneAndUpdateAsync(
                BuildVersionedFilter(domainEvent),
                BuildVersionUpdate(domainEvent)
                    .Set(user => user.SecurityStamp, $"{domainEvent.Identity}")
            );
        }
    }
}