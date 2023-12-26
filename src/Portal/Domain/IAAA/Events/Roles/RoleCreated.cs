using Eventually.Interfaces.DomainEvents;

namespace Eventually.Portal.Domain.IAAA.Events.Roles
{
    public class RoleCreated : DomainEntityCreatedEventBase, IRoleEvent
    {
        public string RoleName { get; }
    }
}