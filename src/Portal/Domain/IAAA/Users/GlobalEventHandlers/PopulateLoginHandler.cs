using Eventually.Domain.EventHandling;
using Eventually.Portal.Domain.IAAA.Events.Users;
using Microsoft.Extensions.Logging;

namespace Eventually.Portal.Domain.IAAA.Users.GlobalEventHandlers
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