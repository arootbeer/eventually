using Eventually.Interfaces.DomainEvents.IAAA.Roles;
using Eventually.Portal.UI.Areas.Identity.Data;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace Eventually.Portal.UI.Areas.Identity.EventHandlers.IAAA
{
    public class RoleRemovedFromUserEventHandler : MongoDomainEventHandler<RoleRemovedFromUser>
    {
        private readonly IMongoCollection<PortalRole> _roles;
        private readonly IMongoCollection<PortalUser> _users;

        public RoleRemovedFromUserEventHandler(IMongoDatabase mongo, ILoggerFactory loggerFactory) : base(mongo, loggerFactory)
        {
            _roles = GetCollection<PortalRole>();
            _users = GetCollection<PortalUser>();
        }

        protected override void HandleInternal(RoleRemovedFromUser domainEvent)
        {
            _roles.FindOneAndUpdateAsync(
                role => role.Id == domainEvent.EntityId,
                Builders<PortalRole>.Update
                    .Pull(role => role.UserIds, domainEvent.UserId)
            );

            _users.FindOneAndUpdateAsync(
                user => user.Id == domainEvent.UserId,
                Builders<PortalUser>.Update
                    .Unset(user => user.Roles[$"{domainEvent.EntityId}"])
                    .Set(user => user.SecurityStamp, $"{domainEvent.Identity}")
            );
        }
    }
}