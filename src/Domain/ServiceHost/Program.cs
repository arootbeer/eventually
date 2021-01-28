using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Eventually.Domain.ServiceHost.Configuration;
using Eventually.Infrastructure.EventStore;
using Eventually.Infrastructure.EventStore.Configuration;
using Eventually.Infrastructure.EventStore.DI;
using Eventually.Infrastructure.Transport.DI;
using Eventually.Utilities.Logging;
using Eventually.Portal.Infrastructure.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Eventually.Domain.ServiceHost
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            await CreateHostBuilder(args)
                .Build()
                .RunAsync();
        }

        private static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .UseWindowsService()
                .ConfigureServices(
                    (context, services) =>
                    {
                        services.AddOptions();

                        services.RegisterConfiguration<IDomainServiceHostConfiguration, DomainServiceHostConfiguration>(context.Configuration, "service");
                        services
                            .AddSingleton(provider => provider.GetService<IDomainServiceHostConfiguration>().EventStore)
                            .AddSingleton(provider => provider.GetService<IDomainServiceHostConfiguration>().DomainData)
                            .AddTransient<IDbConnection>(sp => new NpgsqlConnection(sp.GetService<IOrmConfiguration>().ConnectionString));
                        services.AddHostedService<DomainServiceHost>();
                        services.AddHostedService<EventStoreListenerService>();
                    }
                )
                .ConfigureAppConfiguration(
                    (context, builder) =>
                    {
                        var env = context.HostingEnvironment;
                        builder.SetBasePath(AppContext.BaseDirectory)
                            .AddJsonFile("appsettings.json", optional: false)
                            .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                            // Override config by env, using like Logging:Level or Logging__Level
                            .AddEnvironmentVariables();
                    }
                )
                .ConfigureLogging((context, logging) =>
                    {
                        log4net.GlobalContext.Properties["logname"] = typeof(DomainServiceHost).Name;
                        logging.AddLog4Net();

                        ApplicationLogging.LoggerFactory = logging.Services.BuildServiceProvider().GetService<ILoggerFactory>();
                    }
                )
                .UseEventStore(customConfiguration: services => services.AddSingleton<ILastCommitCheckpointProvider, DomainServiceHostCheckpointProvider>())
                .UseNetMQMessageTransport();
        }

        //For now, domain services can be populated from the beginning on startup
        private class DomainServiceHostCheckpointProvider : ILastCommitCheckpointProvider
        {
            private int _token;

            public long GetLastCheckpointToken(string bucketId)
            {
                return _token;
            }

            public void SetLastCheckpointToken(string bucketId, long newToken)
            {
                Interlocked.Increment(ref _token);
            }
        }
    }
}