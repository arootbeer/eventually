using Eventually.Infrastructure.Configuration;
using Eventually.Infrastructure.EventStore;
using Eventually.Infrastructure.EventStore.Configuration;
using Eventually.Infrastructure.EventStore.DI;
using Eventually.Infrastructure.Transport.DI;
using Eventually.Portal.Domain.IAAA.Users;
using Eventually.Portal.UI.Configuration;
using Eventually.Portal.UI.ViewModel;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MongoDb.Bson.NodaTime;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;

namespace Eventually.Portal.UI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args)
                .Build()
                .Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .UseWindowsService()
                .ConfigureServices((context, services) =>
                    {
                        services.AddOptions();

                        services.RegisterConfiguration<IServerUIConfiguration, ServerUIConfiguration>(
                            context.Configuration, "server.ui");
                        services.RegisterConfiguration<IEventStoreConfiguration, EventStoreConfiguration>(
                            context.Configuration, "eventStore");
                        services
                            .AddSingleton(container =>
                            {
                                var settings = container.GetService<IServerUIConfiguration>().ViewModelDatabase;
                                return new MongoClient(
                                        $"mongodb://{settings.User}:{settings.Password}@{settings.Address}:{settings.Port}")
                                    .GetDatabase(settings.DatabaseName);
                            })
                            .AddTransient<IUserLoginHashGenerator, UserLoginHashGenerator>();
                    }
                )
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.ConfigureKestrel((context, options) =>
                    {
                        var hostConfig = context.Configuration.Bind<ServerUIConfiguration>("server.ui");
                        options.ListenAnyIP(443, listenOptions =>
                        {
                            listenOptions.UseHttps(
                                hostConfig.CertificateFilePath,
                                context.Configuration["CertPassword"]
                            );
                        });
                        options.ListenAnyIP(80);
                    });
                    
                    webBuilder.UseStartup<Startup>();
                })
                .ConfigureServices(services =>
                {
                    //prevent Mongo from complaining when objects don't have an explicitly mapped id field
                    ConventionRegistry.Register(
                        "IgnoreExtraElements",
                        new ConventionPack {new IgnoreExtraElementsConvention(true)},
                        _ => true
                    );
                    
                    NodaTimeSerializers.Register();
                    services.AddHostedService<EventStoreListenerService>();
                })
                .UseEventStore(
                    new[] {typeof(Program).Assembly},
                    services =>
                    {
                        services.AddSingleton<ILastCommitCheckpointProvider, MongoLastCommitCheckpointProvider>();
                    }
                )
                .UseHTTPDomainCommandTransport();
        }
    }
}
