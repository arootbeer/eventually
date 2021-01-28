using Eventually.Interfaces.DomainEvents.IAAA.Users;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace Eventually.Portal.UI.Areas.Identity.EventHandlers.IAAA.Users
{
    public class LoginAddedToUserEventHandler : MongoDomainEventHandler<LoginAddedToUser, PortalUserLogin>
    {
        public LoginAddedToUserEventHandler(IMongoDatabase mongo, ILoggerFactory loggerFactory) : base(mongo, loggerFactory) { }

        protected override void HandleInternal(LoginAddedToUser domainEvent)
        {
            Collection.FindOneAndReplaceAsync(
                Filter.And(
                    Filter.Eq(login => login.UserId, domainEvent.EntityId),
                    Filter.Eq(login => login.LoginProvider, domainEvent.LoginProvider)
                ),
                new PortalUserLogin
                {
                    UserId = domainEvent.EntityId,
                    LoginProvider = domainEvent.LoginProvider,
                    LoginHash = domainEvent.LoginHash
                },
                new FindOneAndReplaceOptions<PortalUserLogin>
                {
                    IsUpsert = true
                }
            );
        }
    }
}