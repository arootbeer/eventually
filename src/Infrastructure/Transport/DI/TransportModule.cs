using Eventually.Infrastructure.Transport.Messages;
using Eventually.Infrastructure.Transport.Sockets;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Eventually.Infrastructure.Transport.DI
{
    public static class Extensions
    {
        public static IHostBuilder UseNetMQMessageTransport(this IHostBuilder hostBuilder)
        {
            return hostBuilder.ConfigureServices((context, services) =>
                {
                    services.AddSingleton<ISocketFactory, SocketFactory>();
                    services.AddSingleton<IWireMessageFactory, WireMessageFactory>();
                }
            );
        }
    }
}