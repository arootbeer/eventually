using System;
using Eventually.Portal.Infrastructure.Configuration;
using NetMQ;
using NetMQ.Sockets;

namespace Eventually.Infrastructure.Transport.Sockets
{
    public interface ISocketFactory
    {
        RouterSocket GetServerSocket(
            Guid myId,
            NetMQPoller poller,
            EventHandler<NetMQSocketEventArgs> messageReceived,
            ISocketAddress socketAddress = null
        );

        DealerSocket GetClientSocket(
            Guid myId,
            Guid remoteId,
            NetMQPoller poller,
            EventHandler<NetMQSocketEventArgs> messageReceived
        );

        DealerSocket GetClientSocket(
            Guid myId,
            ISocketAddress socketAddress,
            NetMQPoller poller,
            EventHandler<NetMQSocketEventArgs> messageReceived
        );
    }
}