using System.Net.Http;

namespace Eventually.Infrastructure.Transport.CommandBus.Configuration
{
    public interface IHttpDomainCommandBusConfiguration
    {
        string ServerUrl { get; }
        
        string APIPath { get; }
        
        void ApplyTo(HttpClient client);
    }
}