using System;
using System.Data;
using System.Threading;
using Eventually.Domain.APIHost.Configuration;
using Eventually.Infrastructure.Configuration;
using Eventually.Infrastructure.EventStore;
using Eventually.Infrastructure.EventStore.Configuration;
using Eventually.Infrastructure.EventStore.DI;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Npgsql;

namespace Eventually.Domain.APIHost
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        private static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((context, services) =>
                    {
                        services.RegisterConfiguration<IDomainAPIHostConfiguration, DomainAPIHostConfiguration>(
                            context.Configuration,
                            "APIHost"
                        );

                        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
                        services
                            .AddSingleton(provider => provider.GetService<IDomainAPIHostConfiguration>().EventStore)
                            .AddSingleton(provider => provider.GetService<IDomainAPIHostConfiguration>().DomainData)
                            .AddTransient<IDbConnection>(provider => new NpgsqlConnection(provider.GetService<IOrmConfiguration>().ConnectionString));

                        services.AddHostedService<EventStoreListenerService>();
                    }
                )
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.ConfigureKestrel((context, options) =>
                    {
                        var hostConfig = context.Configuration.Bind<DomainAPIHostConfiguration>("APIHost");
                        options.ListenAnyIP(5001, listenOptions =>
                        {
                            listenOptions.UseHttps(
                                hostConfig.CertificateFilePath,
                                context.Configuration["CertPassword"]
                            );
                        });
                        options.ListenAnyIP(5000);
                    });
                    
                    webBuilder.UseStartup<Startup>();
                })
                .UseEventStore(customConfiguration: services =>
                    services.AddSingleton<ILastCommitCheckpointProvider, DomainAPIHostCheckpointProvider>());
        
        
        //For now, domain services can be populated from the beginning on startup
        private class DomainAPIHostCheckpointProvider : ILastCommitCheckpointProvider
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