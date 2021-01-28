using Eventually.Interfaces.DomainEvents.IAAA.Roles;
using Eventually.Portal.UI.Areas.Identity.Data;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace Eventually.Portal.UI.Areas.Identity.EventHandlers.IAAA.Users
{
    public class RoleNameChangedEventHandler : MongoVersionedDomainEventHandler<RoleNameChanged, PortalUser>
    {
        public RoleNameChangedEventHandler(IMongoDatabase database, ILoggerFactory loggerFactory) : base(database, loggerFactory) { }

        protected override void HandleInternal(RoleNameChanged domainEvent)
        {
            var usersInRole = Collection.Find(user => user.Roles.ContainsKey($"{domainEvent.EntityId}")).ToEnumerable();

            foreach (var existing in usersInRole)
            {
                existing.Roles[$"{domainEvent.EntityId}"] = domainEvent.RoleName;
                Collection.ReplaceOneAsync(user => user.Id == existing.Id, existing);
            }
        }
    }
}