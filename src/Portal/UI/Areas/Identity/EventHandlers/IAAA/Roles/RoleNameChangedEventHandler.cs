using Eventually.Interfaces.DomainEvents.IAAA.Roles;
using Eventually.Portal.UI.Areas.Identity.Data;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace Eventually.Portal.UI.Areas.Identity.EventHandlers.IAAA.Roles
{
    public class RoleNameChangedEventHandler : MongoVersionedDomainEventHandler<RoleNameChanged, ServerUIRole>
    {
        public RoleNameChangedEventHandler(IMongoDatabase database, ILoggerFactory loggerFactory) : base(database, loggerFactory) { }

        protected override void HandleInternal(RoleNameChanged domainEvent)
        {
            Collection.FindOneAndUpdateAsync(
                BuildVersionedFilter(domainEvent),
                BuildVersionUpdate(domainEvent)
                    .Set(role => role.Name, domainEvent.RoleName)
            );
        }
    }
}