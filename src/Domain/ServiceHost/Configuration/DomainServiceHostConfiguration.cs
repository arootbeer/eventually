using System;
using Eventually.Infrastructure.EventStore.Configuration;
using Eventually.Portal.Infrastructure.Configuration;

namespace Eventually.Domain.ServiceHost.Configuration
{
    public class DomainServiceHostConfiguration : IDomainServiceHostConfiguration
    {
        public Guid Identity { get; set; }

        public SocketAddress Server { get; set; }
        
        ISocketAddress IDomainServiceHostConfiguration.Server => Server;

        public EventStoreConfiguration EventStore { get; set; }
        IEventStoreConfiguration IDomainServiceHostConfiguration.EventStore => EventStore;

        public OrmConfiguration DomainData { get; set; }
        IOrmConfiguration IDomainServiceHostConfiguration.DomainData => DomainData;
    }
}