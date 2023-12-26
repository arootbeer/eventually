using Eventually.Portal.Domain.IAAA.Events.Users;
using Eventually.Portal.UI.Areas.Identity.Data;
using Eventually.Portal.UI.EventHandlers;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace Eventually.Portal.UI.Areas.Identity.EventHandlers.IAAA.Users
{
    public class LoginRemovedFromUserEventHandler : MongoDomainEventHandler<LoginRemovedFromUser, PortalUserLogin>
    {
        public LoginRemovedFromUserEventHandler(IMongoDatabase mongo, ILoggerFactory loggerFactory) : base(mongo, loggerFactory) { }

        protected override void HandleInternal(LoginRemovedFromUser domainEvent)
        {
            Collection.FindOneAndDeleteAsync(
                Filter.And(
                    Filter.Eq(login => login.UserId, domainEvent.EntityId),
                    Filter.Eq(login => login.LoginProvider, domainEvent.LoginProvider),
                    Filter.Eq(login => login.LoginHash, domainEvent.LoginHash)
                )
            );
        }
    }
}