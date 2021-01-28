using System;

namespace Eventually.Interfaces.DomainEvents.IAAA.Roles
{
    public class RoleRemovedFromUser : ChangedEventBase, IRoleEvent
    {
        public Guid UserId { get; }
    }
}