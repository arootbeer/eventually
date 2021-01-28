using Eventually.Domain.EventHandlers;
using Eventually.Interfaces.DomainEvents.IAAA.Users;
using Microsoft.Extensions.Logging;

namespace Eventually.Domain.IAAA.Users.EventHandlers
{
    public class PopulateLoginHandler : DomainEventHandlerBase<UserCreated>
    {
        public PopulateLoginHandler(ILoggerFactory loggerFactory) : base(loggerFactory)
        {
        }

        protected override void HandleInternal(UserCreated userCreated)
        {
            User.ExistingUserLogins.Add(userCreated.LoginHash);
        }
    }
}