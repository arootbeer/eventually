using System;
using System.Net.Http;

namespace Eventually.Infrastructure.Transport.CommandBus.Configuration
{
    internal class HttpDomainCommandBusConfiguration : IHttpDomainCommandBusConfiguration
    {
        public string ServerUrl { get; set; }
        
        public string APIPath { get; set; }
        
        public void ApplyTo(HttpClient client)
        {
            client.BaseAddress = new Uri(ServerUrl + APIPath);
        }
    }
}