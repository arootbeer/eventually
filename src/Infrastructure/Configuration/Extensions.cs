using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Eventually.Infrastructure.Configuration
{
    public static class Extensions
    {
        public static T Bind<T>(this IConfiguration rawConfig, string key = null)
            where T : class, new()
        {
            if (key != null)
            {
                rawConfig = rawConfig.GetSection(key);
            }
            var result = new T();
            rawConfig.Bind(result, options => options.BindNonPublicProperties = true);
            return result;
        }
        
        public static void RegisterConfiguration<TInterface, TConcrete>(this IServiceCollection services, IConfiguration rawConfig, string key)
            where TConcrete : class, TInterface, new()
            where TInterface : class
        {
            var result = rawConfig.Bind<TConcrete>(key);
            services.AddSingleton<TInterface>(result);
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