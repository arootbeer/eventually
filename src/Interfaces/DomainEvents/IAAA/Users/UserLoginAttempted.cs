namespace Eventually.Interfaces.DomainEvents.IAAA.Users
{
    public class UserLoginAttempted : ChangedEventBase, IUserEvent 
    {
       public string Result { get; }
        
        public string IPAddress { get; }

        public sealed class LoginAttemptResult
        {
            public const string Succeeded = nameof(Succeeded);
            public const string Failed = nameof(Failed);
            public const string Lockout = nameof(Lockout);
        }
    }
}