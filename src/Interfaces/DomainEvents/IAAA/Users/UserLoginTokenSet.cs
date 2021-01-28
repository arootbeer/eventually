namespace Eventually.Interfaces.DomainEvents.IAAA.Users
{
    public class UserLoginTokenSet : ChangedEventBase, IUserEvent
    {
        public string LoginProvider { get; }
        
        public string TokenName { get; }

        public string TokenValue { get; }
    }
}