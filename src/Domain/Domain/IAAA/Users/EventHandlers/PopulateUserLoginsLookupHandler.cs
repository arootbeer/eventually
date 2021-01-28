using Eventually.Domain.EventHandlers;
using Eventually.Interfaces.DomainEvents.IAAA.Users;
using Microsoft.Extensions.Logging;

namespace Eventually.Domain.IAAA.Users.EventHandlers
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