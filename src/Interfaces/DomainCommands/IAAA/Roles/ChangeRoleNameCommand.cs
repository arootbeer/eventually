namespace Eventually.Interfaces.DomainCommands.IAAA.Roles
{
    public record ChangeRoleNameCommand : ChangeEntityCommand
    {
        public string RoleName { get; init; }
    }
}