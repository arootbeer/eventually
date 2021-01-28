using System;
using System.Data.Common;
using Eventually.Infrastructure.EventStore.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
                .LogToOutputWindow()
                .LogToConsoleWindow()
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