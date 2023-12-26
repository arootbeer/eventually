using Eventually.Interfaces.DomainEvents;
using NodaTime;

namespace Eventually.Portal.Domain.IAAA.Events.Users
{
    public class UserLoginAttempted : DomainEntityChangedEventBase, IUserEvent 
    {
       public string Result { get; }
        
        public string IPAddress { get; }
        
        public Instant LockoutEnd { get; }

        public sealed class LoginAttemptResult
        {
            public const string Succeeded = nameof(Succeeded);
            public const string Failed = nameof(Failed);
            public const string Lockout = nameof(Lockout);
        }
    }
}