// using Eventually.Interfaces.DomainEvents.IAAA.Users;
// using Eventually.Portal.UI.Areas.Identity.Data;
// using Microsoft.Extensions.Logging;
// using MongoDB.Driver;
//
// namespace Eventually.Portal.UI.Areas.Identity.EventHandlers.IAAA.Users
// {
//     public class UserLoginTokenSetEventHandler : MongoDomainEventHandler<UserLoginTokenSet, PortalUserLogin>
//     {
//         public UserLoginTokenSetEventHandler(IMongoDatabase mongo, ILoggerFactory loggerFactory) : base(mongo, loggerFactory) { }
//
//         protected override void HandleInternal(UserLoginTokenSet domainEvent)
//         {
//             Collection.FindOneAndUpdateAsync(
//                 Filter.And(
//                     Filter.Eq(login => login.UserId, domainEvent.EntityId),
//                     Filter.Eq(login => login.LoginProvider, domainEvent.LoginProvider)
//                     )
//             );
//         }
//     }
// }