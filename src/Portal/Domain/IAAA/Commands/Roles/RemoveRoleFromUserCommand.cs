using System;
using Eventually.Interfaces.DomainCommands;

namespace Eventually.Portal.Domain.IAAA.Commands.Roles
{
    public record RemoveRoleFromUserCommand : ChangeEntityCommand
    {
        public Guid UserId { get; init; }
    }
}