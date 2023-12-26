using System.Threading;
using System.Threading.Tasks;
using Eventually.Interfaces.DomainEvents;
using Eventually.Portal.UI.Areas.Identity.Data;
using Eventually.Portal.UI.ViewModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace Eventually.Portal.UI.EventHandlers
{
    public abstract class PortalUIEventHandler<TEvent, TViewModel> : MongoVersionedDomainEventHandler<TEvent, TViewModel>
        where TEvent : class, IDomainEvent
        where TViewModel : class, IViewModel
    {
        private readonly IUserStore<PortalUser> _userStore;

        protected PortalUIEventHandler(
            IUserStore<PortalUser> userStore,
            IMongoDatabase database,
            ILoggerFactory loggerFactory
        ) : base(database, loggerFactory)
        {
            _userStore = userStore;
        }

        protected async Task<PortalUser> GetUserFor(TEvent domainEvent, CancellationToken cancellationToken = default)
        {
            return await _userStore.FindByIdAsync(domainEvent.IssuingUserId.ToString(), cancellationToken);
        }
    }
}