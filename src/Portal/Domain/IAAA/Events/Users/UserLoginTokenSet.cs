using Eventually.Interfaces.DomainEvents;

namespace Eventually.Portal.Domain.IAAA.Events.Users
{
    public class UserLoginTokenSet : DomainEntityChangedEventBase, IUserEvent
    {
        public string LoginProvider { get; }
        
        public string TokenName { get; }

        public string TokenValue { get; }
    }
}