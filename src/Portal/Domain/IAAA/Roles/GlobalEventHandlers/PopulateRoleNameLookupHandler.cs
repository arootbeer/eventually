using Eventually.Domain.EventHandling;
using Eventually.Portal.Domain.IAAA.Events.Roles;
using Microsoft.Extensions.Logging;

namespace Eventually.Portal.Domain.IAAA.Roles.GlobalEventHandlers
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