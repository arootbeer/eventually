namespace Eventually.Interfaces.DomainEvents.IAAA.Users
{
    public class UserCreated : CreatedEventBase, IUserEvent
    {
        public string Username { get; }
        
        public string LoginHash { get; }

        public byte[] SaltedPasswordHash { get; }

        public byte[] PasswordSalt { get; }
    }
}