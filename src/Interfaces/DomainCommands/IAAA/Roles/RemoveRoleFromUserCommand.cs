using System;

namespace Eventually.Interfaces.DomainCommands.IAAA.Roles
{
    public class RemoveRoleFromUserCommand : ChangeEntityCommand
    {
        public Guid UserId { get; }
    }
}