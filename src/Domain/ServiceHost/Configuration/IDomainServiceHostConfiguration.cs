using Eventually.Infrastructure.EventStore.Configuration;
using Eventually.Portal.Infrastructure.Configuration;

namespace Eventually.Domain.ServiceHost.Configuration
{
    public interface IDomainServiceHostConfiguration : IUniquelyIdentifiedConfiguration
    {
        ISocketAddress Server { get; }
        
        IEventStoreConfiguration EventStore { get; }
        
        IOrmConfiguration DomainData { get; }
    }
}