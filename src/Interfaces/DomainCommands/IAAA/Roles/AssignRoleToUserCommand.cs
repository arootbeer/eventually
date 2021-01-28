using System;

namespace Eventually.Interfaces.DomainCommands.IAAA.Roles
{
    public class AssignRoleToUserCommand : ChangeEntityCommand
    {
        public Guid UserId { get; }
    }
}