using Eventually.Portal.Domain.IAAA.Events.Users;
using Eventually.Portal.UI.Areas.Identity.Data;
using Eventually.Portal.UI.EventHandlers;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace Eventually.Portal.UI.Areas.Identity.EventHandlers.IAAA.Users
{
    public class UserCreatedEventHandler : MongoVersionedDomainEventHandler<UserCreated, PortalUser>
    {
        public UserCreatedEventHandler(IMongoDatabase database, ILoggerFactory loggerFactory) : base(database, loggerFactory) { }

        protected override void HandleInternal(UserCreated domainEvent)
        {
            var user = new PortalUser
            {
                Id = domainEvent.EntityId,
                UserName = domainEvent.Username,
                SecurityStamp = $"{domainEvent.Identity}",
                Active = true
            };

            Collection.InsertOne(user);
        }
    }
}