using Eventually.Interfaces.DomainEvents.IAAA.Users;
using Eventually.Portal.UI.Areas.Identity.Data;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace Eventually.Portal.UI.Areas.Identity.EventHandlers.IAAA.Users
{
    public class UserLoginAttemptedEventHandler : MongoVersionedDomainEventHandler<UserLoginAttempted, PortalUser>
    {
        public UserLoginAttemptedEventHandler(IMongoDatabase mongo, ILoggerFactory loggerFactory) : base(mongo, loggerFactory) { }

        protected override void HandleInternal(UserLoginAttempted domainEvent)
        {
            Collection.FindOneAndUpdateAsync(
                BuildVersionedFilter(domainEvent),
                BuildVersionUpdate(domainEvent)
                    .Set(user => user.SecurityStamp, $"{domainEvent.Identity}")
            );
        }
    }
}