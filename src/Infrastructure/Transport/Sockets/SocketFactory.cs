using System;
using System.Net;
using Eventually.Portal.Infrastructure.Configuration;
using Microsoft.Extensions.Logging;
using NetMQ;
using NetMQ.Sockets;
using SocketAddress = Eventually.Portal.Infrastructure.Configuration.SocketAddress;

namespace Eventually.Infrastructure.Transport.Sockets
{
    public class SocketFactory : ISocketFactory
    {
        private readonly ILogger<SocketFactory> _logger;

        public SocketFactory(ILogger<SocketFactory> logger)
        {
            _logger = logger;
        }

        public RouterSocket GetServerSocket(
            Guid myId,
            NetMQPoller poller,
            EventHandler<NetMQSocketEventArgs> messageReceived,
            ISocketAddress socketAddress = null
        )
        {
            socketAddress ??= new SocketAddress
            {
                Protocol = "tcp",
                Address = Localhost.ToString(),
                Port = ExtractPort(myId)
            };
            var address = $"{socketAddress.Protocol}://{socketAddress.Address}:{socketAddress.Port}";

            _logger.LogInformation($"Creating a server socket for `{myId}` that will bind `{address}`");
            try
            {
                var socket = new RouterSocket();
                socket.Options.Identity = myId.ToByteArray();
                socket.Options.Linger = TimeSpan.FromSeconds(10);
                poller.Add(socket);
                socket.ReceiveReady += messageReceived;
                socket.Bind(address);
                _logger.LogDebug($"Server socket bound `{address}` successfully");
                return socket;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unable to bind `{address}`", ex);
                throw;
            }
        }

        public DealerSocket GetClientSocket(
            Guid myId,
            Guid remoteId,
            NetMQPoller poller,
            EventHandler<NetMQSocketEventArgs> messageReceived
        )
        {
            return GetClientSocket(
                myId,
                new SocketAddress
                {
                    Protocol = "tcp",
                    Address = Localhost.ToString(),
                    Port = ExtractPort(remoteId)
                },
                poller,
                messageReceived
            );
        }

        public DealerSocket GetClientSocket(
            Guid myId,
            ISocketAddress socketAddress,
            NetMQPoller poller,
            EventHandler<NetMQSocketEventArgs> messageReceived
        )
        {
            var address = $"{socketAddress.Protocol}://{socketAddress.Address}:{socketAddress.Port}";

            _logger.LogInformation($"Creating a client socket for `{myId}` that will connect to `{address}`");
            try
            {
                var socket = new DealerSocket();
                socket.Options.Identity = myId.ToByteArray();
                socket.Options.Linger = TimeSpan.FromSeconds(10);
                poller.Add(socket);
                socket.ReceiveReady += messageReceived;
                socket.Connect(address);
                _logger.LogDebug($"Client socket connected to `{address}` successfully");
                return socket;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unable to connect to `{address}`", ex);
                throw;
            }
        }

        private static IPAddress Localhost => IPAddress.Loopback;

        private static int ExtractPort(Guid id)
        {
            return BitConverter.ToUInt16(id.ToByteArray(), 0) | 0xf000;
        }
    }
}