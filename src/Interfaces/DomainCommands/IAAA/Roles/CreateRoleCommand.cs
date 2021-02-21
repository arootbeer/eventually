namespace Eventually.Interfaces.DomainCommands.IAAA.Roles
{
    public record CreateRoleCommand : CreateEntityCommand
    {
        public string RoleName { get; init; }
    }
}