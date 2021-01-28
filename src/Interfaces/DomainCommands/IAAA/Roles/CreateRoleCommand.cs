namespace Eventually.Interfaces.DomainCommands.IAAA.Roles
{
    public class CreateRoleCommand : CreateEntityCommand
    {
        public string RoleName { get; }
    }
}