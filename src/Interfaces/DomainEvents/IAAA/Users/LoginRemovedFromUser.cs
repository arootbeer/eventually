namespace Eventually.Interfaces.DomainEvents.IAAA.Users
{
    public class LoginRemovedFromUser : ChangedEventBase, IUserEvent
    {
        public string LoginProvider { get; }
        
        public string LoginHash { get; }
    }
}