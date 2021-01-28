using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Eventually.Portal.Infrastructure.Configuration
{
    public static class Extensions
    {
        public static void RegisterConfiguration<TInterface, TConcrete>(this IServiceCollection services, IConfiguration rawConfig, string key)
            where TConcrete : TInterface, new()
            where TInterface : class
        {
            var configuration = new TConcrete();
            rawConfig.Bind(key, configuration);
            services.AddSingleton<TInterface>(configuration);
        }

        public static void RegisterWritableConfiguration<T>(
            this IServiceCollection services,
            IConfiguration configuration,
            string key,
            string file = "appsettings.json"
        )
            where T : class, new()
        {
            services.RegisterConfiguration<T, T>(configuration, key);
            services.AddSingleton<IWritableOptions<T>>(provider =>
                {
                    var environment = provider.GetService<IHostEnvironment>();
                    var options = provider.GetService<IOptionsMonitor<T>>();
                    return new WritableOptions<T>(environment, options, key, file);
                }
            );
            services.AddSingleton<IOptions<T>>(provider => provider.GetService<IWritableOptions<T>>());
        }
    }
}