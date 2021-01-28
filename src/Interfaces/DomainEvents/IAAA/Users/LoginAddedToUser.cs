namespace Eventually.Interfaces.DomainEvents.IAAA.Users
{
    public class LoginAddedToUser : ChangedEventBase, IUserEvent
    {
        public string LoginProvider { get; }
        
        public string LoginHash { get; }

        public string ProviderDisplayName { get; }
    }
}