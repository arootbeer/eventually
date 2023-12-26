using Eventually.Portal.Domain.IAAA.Events.Roles;
using Eventually.Portal.UI.Areas.Identity.Data;
using Eventually.Portal.UI.EventHandlers;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace Eventually.Portal.UI.Areas.Identity.EventHandlers.IAAA.Roles
{
    public class RoleCreatedEventHandler : MongoVersionedDomainEventHandler<RoleCreated, PortalRole>
    {
        public RoleCreatedEventHandler(IMongoDatabase database, ILoggerFactory loggerFactory) : base(database, loggerFactory) { }

        protected override void HandleInternal(RoleCreated domainEvent)
        {
            var role = new PortalRole
            {
                Id = domainEvent.EntityId,
                Name = domainEvent.RoleName
            };

            Collection.InsertOne(role);
        }
    }
}