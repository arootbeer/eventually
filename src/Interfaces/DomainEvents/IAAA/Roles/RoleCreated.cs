namespace Eventually.Interfaces.DomainEvents.IAAA.Roles
{
    public class RoleCreated : CreatedEventBase, IRoleEvent
    {
        public string RoleName { get; }
    }
}