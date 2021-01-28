using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Eventually.Infrastructure.Transport.Extensions;
using Eventually.Infrastructure.Transport.Messages;
using Eventually.Infrastructure.Transport.Sockets;
using Eventually.Interfaces.Common;
using Eventually.Interfaces.Common.Messages;
using Eventually.Interfaces.DomainCommands;
using Eventually.Portal.UI.Areas.Identity;
using Eventually.Portal.UI.Configuration;
using Eventually.Utilities.Extensions;
using Fasterflect;
using Microsoft.Extensions.Logging;
using NetMQ;
using NetMQ.Sockets;

namespace Eventually.Portal.UI.Domain
{
    public class DomainCommandBus : IDomainCommandBus, IUniquelyIdentified
    {
        private readonly BlockingCollection<IMessage> _outboundQueue = new BlockingCollection<IMessage>();

        private readonly ConcurrentDictionary<Guid, BlockingCollection<DomainCommandResponse>> _responseBucket =
            new ConcurrentDictionary<Guid, BlockingCollection<DomainCommandResponse>>();

        private readonly IWireMessageFactory _wireMessageFactory;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly DealerSocket _socket;
        private readonly NetMQPoller _poller;
        private readonly Task _messagePump;
        private readonly ILogger<DomainCommandBus> _logger;

        public Guid Identity { get; }

        public DomainCommandBus(
            IWireMessageFactory wireMessageFactory,
            ISocketFactory socketFactory,
            IServerUIConfiguration configuration,
            ILogger<DomainCommandBus> logger
        )
        {
            Identity = Guid.NewGuid();

            _cancellationTokenSource = new CancellationTokenSource();
            _logger = logger;

            _poller = new NetMQPoller();
            _socket = socketFactory.GetClientSocket(Identity, configuration.ServerAddress, _poller, ResponseReceived);
            _wireMessageFactory = wireMessageFactory;

            _messagePump = Task.Factory
                .StartNew(() =>
                    {
                        while (!_cancellationTokenSource.IsCancellationRequested)
                            try
                            {
                                var command = _outboundQueue.Take(_cancellationTokenSource.Token);
                                var method = _messageBuilderMethod.MakeGenericMethod(command.GetType());
                                var message = (IWireMessage) method.Invoke(this, new object [] {command});
                                _socket.SendMessage(message);
                            }
                            catch (OperationCanceledException)
                            {
                            }
                    },
                    _cancellationTokenSource.Token,
                    TaskCreationOptions.LongRunning,
                    TaskScheduler.Default
                );

            _poller.RunAsync();
        }

        private static readonly MethodInfo _messageBuilderMethod = typeof(DomainCommandBus)
            .Methods(nameof(BuildTypedWireMessage))
            .Single();
        private IWireMessage BuildTypedWireMessage<TMessage>(TMessage message) where TMessage : IMessage
        {
            return _wireMessageFactory.Create(message, this);
        }

        private void ResponseReceived(object sender, NetMQSocketEventArgs e)
        {
            var netMQMessage = e.Socket.ReceiveMultipartMessage();
            var wireMessage = _wireMessageFactory.Create(netMQMessage);
            var message = wireMessage.Message;

            try
            {
                if (!(message is DomainCommandResponse response))
                {
                    throw new Exception(
                        $"Expecting a command response, but received a response of type `{message.GetType()}`. " +
                        $"The message is:{Environment.NewLine}```{message.ToJson()}```"
                    );
                }

                _responseBucket.TryGetValue(wireMessage.CommandOrQueryId, out var responses);
                if (responses != null)
                    responses.Add(response);
                else
                    _logger.LogWarning(
                        $"Received response message of type `{wireMessage.MessageType.FullName}`, but no handler was registered; ignoring. " +
                        $"The message is:{Environment.NewLine}```{message.ToJson()}```"
                    );
            }
            catch (OperationCanceledException ocex)
            {
                if (!_cancellationTokenSource.IsCancellationRequested)
                {
                    throw new Exception("Caught an OperationCanceledException but the cancellation token is not canceled. Did someone call _responseBucket[commandIdentity].CompleteAdding() by accident?", ocex);
                }

                throw;
            }
        }

        public async Task ExecuteCommand(DomainCommand command, CancellationToken cancellationToken)
        {
            var exception = await ExecuteCommand(
                    command,
                    (response, token) => response.ToException(),
                    cancellationToken
                );

            if (exception != null)
            {
                throw exception;
            }
        }

        public async Task<TResult> ExecuteCommand<TResult>(
            DomainCommand command,
            Func<DomainCommandResponse, CancellationToken, TResult> handler,
            CancellationToken cancellationToken
        )
        {
            _responseBucket.TryAdd(
                command.Identity,
                new BlockingCollection<DomainCommandResponse>(1)
            );

            _outboundQueue.Add(command, cancellationToken);

            return await Task.Factory
                .StartNew(
                    () =>
                    {
                        DomainCommandResponse response = null;
                        if (_responseBucket.ContainsKey(command.Identity))
                        {
                            response = _responseBucket[command.Identity].Take(cancellationToken);
                        }

                        if (_responseBucket.Remove(command.Identity, out var collection))
                        {
                            collection.Dispose();
                        }

                        return handler == null || response == null ? default : handler(response, cancellationToken);
                    },
                    cancellationToken
                );
        }

        private bool _disposed;

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            _logger.LogDebug($"{nameof(UserStore)} is being disposed.");
            GC.SuppressFinalize(this);
            _cancellationTokenSource.Cancel();
            try
            {
                _messagePump.Wait(TimeSpan.FromSeconds(10));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Exception encountered while waiting for message sender to complete");
            }

            _messagePump.Dispose();

            _poller?.Remove(_socket);
            _poller?.Dispose();
            _socket?.Dispose();

            _logger.LogInformation($"{nameof(UserStore)} has been disposed.");
        }
    }
}