using Eventually.Interfaces.DomainEvents;

namespace Eventually.Portal.Domain.IAAA.Events.Roles
{
    public class RoleNameChanged : DomainEntityChangedEventBase, IRoleEvent
    {
        public string RoleName { get; }
    }
}