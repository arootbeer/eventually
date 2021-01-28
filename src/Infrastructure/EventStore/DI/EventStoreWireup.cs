using System;
using System.Data.Common;
using Eventually.Infrastructure.EventStore.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NEventStore;
using NEventStore.Persistence.Sql.SqlDialects;
using Npgsql;

namespace Eventually.Infrastructure.EventStore.DI
{
    public sealed class EventStoreWireup
    {
        static EventStoreWireup()
        {
            DbProviderFactories.RegisterFactory("Npgsql", NpgsqlFactory.Instance);
        }

        public static Func<IServiceProvider, IStoreEvents> Factory =>
            sp => Wireup.Init()
                .WithLoggerFactory(sp.GetService<ILoggerFactory>())
                .UseOptimisticPipelineHook()
                .UsingSqlPersistence(
                    DbProviderFactories.GetFactory("Npgsql"),
                    sp.GetService<IEventStoreConfiguration>().ConnectionString
                )
                .WithDialect(new PostgreSqlDialect())
                .InitializeStorageEngine()
                .UsingCustomSerialization(new DomainEventJsonSerializer())
                .HookIntoPipelineUsing(new AuthorizationPipelineHook())
                .Build();
    }
}