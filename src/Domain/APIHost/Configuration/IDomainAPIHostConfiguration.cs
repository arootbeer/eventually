using Eventually.Infrastructure.EventStore.Configuration;

namespace Eventually.Domain.APIHost.Configuration
{
    public interface IDomainAPIHostConfiguration
    {
        IEventStoreConfiguration EventStore { get; }
        
        IOrmConfiguration DomainData { get; }
        
        string CertificateFilePath { get; }
    }
}