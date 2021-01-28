namespace Eventually.Interfaces.DomainEvents.IAAA.Roles
{
    public class RoleNameChanged : ChangedEventBase, IRoleEvent
    {
        public string RoleName { get; }
    }
}