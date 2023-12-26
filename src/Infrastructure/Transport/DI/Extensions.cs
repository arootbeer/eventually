using Eventually.Infrastructure.Configuration;
using Eventually.Infrastructure.Transport.CommandBus;
using Eventually.Infrastructure.Transport.CommandBus.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Eventually.Infrastructure.Transport.DI
{
    public static class Extensions
    {
        public static IHostBuilder UseHTTPDomainCommandTransport(this IHostBuilder hostBuilder)
        {
            return hostBuilder.ConfigureServices((context, services) =>
                {
                    services.RegisterConfiguration<IHttpDomainCommandBusConfiguration, HttpDomainCommandBusConfiguration>(
                        context.Configuration, "domain.CommandBus");

                    services.AddHttpClient()
                        .AddSingleton<IDomainCommandBus, HTTPDomainCommandBus>();
                }
            );
        }
    }
}