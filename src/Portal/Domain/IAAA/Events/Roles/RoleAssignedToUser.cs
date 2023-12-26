using System;
using Eventually.Interfaces.DomainEvents;

namespace Eventually.Portal.Domain.IAAA.Events.Roles
{
    public class RoleAssignedToUser : DomainEntityChangedEventBase, IRoleEvent
    {
        public Guid UserId { get; }
    }
}