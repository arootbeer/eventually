using Eventually.Interfaces.DomainEvents;

namespace Eventually.Portal.Domain.IAAA.Events.Users
{
    public class UserPasswordChanged : DomainEntityChangedEventBase, IUserEvent
    {
        public byte[] SaltedPasswordHash { get; }

        public byte[] PasswordSalt { get; }
    }
}