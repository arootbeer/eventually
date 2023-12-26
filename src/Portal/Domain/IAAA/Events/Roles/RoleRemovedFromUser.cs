using System;
using Eventually.Interfaces.DomainEvents;

namespace Eventually.Portal.Domain.IAAA.Events.Roles
{
    public class RoleRemovedFromUser : DomainEntityChangedEventBase, IRoleEvent
    {
        public Guid UserId { get; }
    }
}