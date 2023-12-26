using Eventually.Interfaces.DomainCommands;

namespace Eventually.Portal.Domain.IAAA.Commands.Roles
{
    public record CreateRoleCommand : CreateEntityCommand
    {
        public string RoleName { get; init; }
    }
}