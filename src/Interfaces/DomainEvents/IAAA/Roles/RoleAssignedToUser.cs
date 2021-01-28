using System;

namespace Eventually.Interfaces.DomainEvents.IAAA.Roles
{
    public class RoleAssignedToUser : ChangedEventBase, IRoleEvent
    {
        public Guid UserId { get; }
    }
}