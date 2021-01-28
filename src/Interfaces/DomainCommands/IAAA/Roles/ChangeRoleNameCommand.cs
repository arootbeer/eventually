namespace Eventually.Interfaces.DomainCommands.IAAA.Roles
{
    public class ChangeRoleNameCommand : ChangeEntityCommand
    {
        public string RoleName { get; }
    }
}