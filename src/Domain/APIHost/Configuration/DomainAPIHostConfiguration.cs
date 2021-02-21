using Eventually.Infrastructure.EventStore.Configuration;

namespace Eventually.Domain.APIHost.Configuration
{
    public class DomainAPIHostConfiguration : IDomainAPIHostConfiguration
    {
        public EventStoreConfiguration EventStore { get; set; }
        IEventStoreConfiguration IDomainAPIHostConfiguration.EventStore => EventStore;

        public OrmConfiguration DomainData { get; set; }
        IOrmConfiguration IDomainAPIHostConfiguration.DomainData => DomainData;

        public string CertificateFilePath { get; private set; }
        string IDomainAPIHostConfiguration.CertificateFilePath => CertificateFilePath;
    }
}