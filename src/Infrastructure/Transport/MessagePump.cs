//using Eventually.Portal.Infrastructure.Configuration;
//using Eventually.Portal.Infrastructure.Transport.Extensions;
//using Eventually.Portal.Infrastructure.Transport.Messages;
//using Eventually.Portal.Infrastructure.Transport.Sockets;
//using NetMQ;
//using NetMQ.Sockets;
//using System.Threading.Tasks;

//namespace Eventually.Portal.Infrastructure.Transport
//{
//    public class MessagePump
//    {
//        private readonly IWireMessageFactory _wireMessageFactory;
//        private readonly DealerSocket _socket;
//        private readonly NetMQPoller _poller;
//        private readonly Task _sender;

//        public MessagePump(
//            ISocketAddress socketAddress,
//            ISocketFactory socketFactory,
            
//            )
//        {
//            _poller = new NetMQPoller();
//            _socket = socketFactory.GetClientSocket(Identity, _configuration.ServerAddress, _poller, ResponseReceived);
//            _wireMessageFactory = wireMessageFactory;

//            _sender = Task.Factory
//                .StartNew(() =>
//                    {
//                        while (true)
//                        {
//                            var message = _wireMessageFactory.Create(_outboundQueue.Take(cancellationTokenSource.Token), this);
//                            _socket.SendMessage(message);
//                        }
//                    },
//                    cancellationTokenSource.Token,
//                    TaskCreationOptions.LongRunning,
//                    TaskScheduler.Default
//                );

//            _poller.RunAsync();

//        }
//    }
//}