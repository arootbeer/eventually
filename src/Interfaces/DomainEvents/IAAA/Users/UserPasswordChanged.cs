namespace Eventually.Interfaces.DomainEvents.IAAA.Users
{
    public class UserPasswordChanged : ChangedEventBase, IUserEvent
    {
        public byte[] SaltedPasswordHash { get; }

        public byte[] PasswordSalt { get; }
    }
}