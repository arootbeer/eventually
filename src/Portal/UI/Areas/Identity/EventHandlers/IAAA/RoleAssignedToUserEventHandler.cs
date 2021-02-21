using Eventually.Interfaces.DomainEvents.IAAA.Roles;
using Eventually.Portal.UI.Areas.Identity.Data;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace Eventually.Portal.UI.Areas.Identity.EventHandlers.IAAA
{
    public class RoleAssignedToUserEventHandler : MongoDomainEventHandler<RoleAssignedToUser>
    {
        private readonly IMongoCollection<PortalRole> _roles;
        private readonly IMongoCollection<PortalUser> _users;

        public RoleAssignedToUserEventHandler(IMongoDatabase mongo, ILoggerFactory loggerFactory) : base(mongo, loggerFactory)
        {
            _roles = GetCollection<PortalRole>();
            _users = GetCollection<PortalUser>();
        }

        protected override void HandleInternal(RoleAssignedToUser domainEvent)
        {
            _roles.FindOneAndUpdateAsync(
                role => role.Id == domainEvent.EntityId,
                Builders<PortalRole>.Update
                    .AddToSet(role => role.UserIds, domainEvent.UserId)
            );

            _users.FindOneAndUpdateAsync(
                user => user.Id == domainEvent.UserId,
                Builders<PortalUser>.Update
                    .Set(
                        user => user.Roles[$"{domainEvent.EntityId}"],
                        _roles.Find(role => role.Id == domainEvent.EntityId).Single().Name
                    )
                    .Set(user => user.SecurityStamp, $"{domainEvent.Identity}")
            );
        }
    }
}