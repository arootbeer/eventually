using System;

namespace Eventually.Interfaces.DomainCommands.IAAA.Roles
{
    public record AssignRoleToUserCommand : ChangeEntityCommand
    {
        public Guid UserId { get; init; }
    }
}