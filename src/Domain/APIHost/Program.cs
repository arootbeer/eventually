using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Data;
using System.Threading;
using Eventually.Domain.APIHost.Configuration;
using Eventually.Infrastructure.Configuration;
using Eventually.Infrastructure.EventStore;
using Eventually.Infrastructure.EventStore.Configuration;
using Eventually.Infrastructure.EventStore.DI;
using Eventually.Interfaces.Common;
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
                .ConfigureServices(
                    (context, services) =>
                    {
                        services.RegisterConfiguration<IDomainAPIHostConfiguration, DomainAPIHostConfiguration>(
                            context.Configuration,
                            "APIHost"
                        );

                        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
                        services
                            .AddSingleton(provider => provider.GetService<IDomainAPIHostConfiguration>().EventStore)
                            .AddSingleton(provider => provider.GetService<IDomainAPIHostConfiguration>().DomainData)
                            .AddSingleton(_ => MessageTypeLookupStrategyProvider.Instance.Strategies)
                            .AddTransient<IDbConnection>(
                                provider => new NpgsqlConnection(
                                    provider.GetService<IOrmConfiguration>().ConnectionString
                                )
                            );

                        services.AddHostedService<EventStoreListenerService>();
                    }
                )
                .ConfigureWebHostDefaults(
                    webBuilder =>
                    {
                        webBuilder.ConfigureKestrel(
                            (context, options) =>
                            {
                                var hostConfig = context.Configuration.Bind<DomainAPIHostConfiguration>("APIHost");
                                options.ListenAnyIP(
                                    5001,
                                    listenOptions =>
                                    {
                                        listenOptions.UseHttps(
                                            hostConfig.CertificateFilePath,
                                            context.Configuration["CertPassword"]
                                        );
                                    }
                                );
                                options.ListenAnyIP(5000);
                            }
                        );

                        webBuilder.UseStartup<Startup>();
                    }
                )
                .UseEventStore(
                    customConfiguration: services =>
                        services.AddSingleton<ILastCommitCheckpointProvider, DomainAPIHostCheckpointProvider>()
                );

        private class MessageTypeLookupStrategyProvider
        {
            [ImportMany(typeof(MessageTypeLookupStrategy))]
            public IEnumerable<MessageTypeLookupStrategy> Strategies { get; set; }

            public static MessageTypeLookupStrategyProvider Instance
            {
                get
                {
                    var instance = new MessageTypeLookupStrategyProvider();
                    var catalog = new DirectoryCatalog(AppContext.BaseDirectory);
                    new CompositionContainer(catalog).SatisfyImportsOnce(instance);
                    return instance;
                }
            }

            private MessageTypeLookupStrategyProvider() { }
        } 
        
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