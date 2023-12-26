using Eventually.Interfaces.DomainCommands;

namespace Eventually.Portal.Domain.IAAA.Commands.Roles
{
    public record ChangeRoleNameCommand : ChangeEntityCommand
    {
        public string RoleName { get; init; }
    }
}