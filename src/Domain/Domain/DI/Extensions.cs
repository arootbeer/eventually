using System;
using System.Collections.Generic;
using System.Reflection;
using Eventually.Domain.Runtime;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Eventually.Domain.DI
{
    public static class Extensions
    {
        public static IHostBuilder UseDefaultDomainCommandHandling(
            this IHostBuilder hostBuilder,
            IEnumerable<Assembly> eventHandlerAssemblies = null,
            Action<IServiceCollection> customConfiguration = null
        )
        {
            hostBuilder.ConfigureServices(
                (_, services) =>
                {
                    services
                        .AddSingleton<IKnownAggregateTypeProvider, KnownAggregateTypeProvider>()
                        .AddSingleton<IDomainCommandExecutor, DomainCommandExecutor>();
                }
            );

            return hostBuilder;
        }
    }
}