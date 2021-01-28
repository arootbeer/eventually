using Eventually.Domain.EventHandlers;
using Eventually.Interfaces.DomainEvents.IAAA.Roles;
using Microsoft.Extensions.Logging;

namespace Eventually.Domain.IAAA.Roles.EventHandlers
{
    public class PopulateRoleNameLookupHandler : DomainEventHandlerBase<RoleCreated>
    {
        public PopulateRoleNameLookupHandler(ILoggerFactory loggerFactory) : base(loggerFactory)
        {
        }

        protected override void HandleInternal(RoleCreated roleCreated)
        {
            Role.ExistingRoleNames.Add(roleCreated.RoleName);
        }
    }
}