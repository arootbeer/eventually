using Eventually.Portal.Domain.IAAA.Events.Users;
using Eventually.Portal.UI.Areas.Identity.Data;
using Eventually.Portal.UI.EventHandlers;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace Eventually.Portal.UI.Areas.Identity.EventHandlers.IAAA.Users
{
    public class UserLoginAttemptedEventHandler : MongoVersionedDomainEventHandler<UserLoginAttempted, PortalUser>
    {
        public UserLoginAttemptedEventHandler(IMongoDatabase mongo, ILoggerFactory loggerFactory) : base(mongo, loggerFactory) { }

        protected override void HandleInternal(UserLoginAttempted domainEvent)
        {
            var user = GetEntityFor(domainEvent);
            var result = domainEvent.Result;
            
            switch (result)
            {
                case UserLoginAttempted.LoginAttemptResult.Succeeded:
                    user.AccessFailedCount = 0;
                    break;
                case UserLoginAttempted.LoginAttemptResult.Failed:
                    user.AccessFailedCount++;
                    break;
                case UserLoginAttempted.LoginAttemptResult.Lockout:
                    user.LockoutEnd = domainEvent.LockoutEnd.ToDateTimeOffset();
                    break;
            }

            Collection.FindOneAndUpdateAsync(
                BuildVersionedFilter(domainEvent),
                BuildVersionUpdate(domainEvent)
                    .Set(u => u.SecurityStamp, $"{domainEvent.Identity}")
                    .Set(u => u.AccessFailedCount, user.AccessFailedCount)
                    .Set(u => u.LockoutEnd, user.LockoutEnd)
            );
        }
    }
}