using System;

namespace Eventually.Interfaces.DomainCommands.IAAA.Roles
{
    public record RemoveRoleFromUserCommand : ChangeEntityCommand
    {
        public Guid UserId { get; init; }
    }
}