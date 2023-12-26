using Eventually.Domain.EventHandling;
using Eventually.Portal.Domain.IAAA.Events.Users;
using Microsoft.Extensions.Logging;

namespace Eventually.Portal.Domain.IAAA.Users.GlobalEventHandlers
{
    public class PopulateUserLoginsLookupHandler : DomainEventHandlerBase<LoginAddedToUser>
    {
        private readonly IUserLoginHashGenerator _userLoginHashGenerator;

        public PopulateUserLoginsLookupHandler(IUserLoginHashGenerator userLoginHashGenerator,
            ILoggerFactory loggerFactory) : base(loggerFactory)
        {
            _userLoginHashGenerator = userLoginHashGenerator;
        }

        protected override void HandleInternal(LoginAddedToUser loginAddedToUser)
        {
            User.ExistingUserLogins.Add(loginAddedToUser.LoginHash);
        }
    }
}