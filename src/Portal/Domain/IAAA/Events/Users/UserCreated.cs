using Eventually.Interfaces.DomainEvents;

namespace Eventually.Portal.Domain.IAAA.Events.Users
{
    public class UserCreated : DomainEntityCreatedEventBase, IUserEvent
    {
        public string Username { get; }
        
        public string LoginHash { get; }

        public byte[] SaltedPasswordHash { get; }

        public byte[] PasswordSalt { get; }
    }
}