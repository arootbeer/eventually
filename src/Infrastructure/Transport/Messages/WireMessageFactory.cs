using System;
using Eventually.Interfaces.Common;
using Eventually.Interfaces.Common.Messages;
using Eventually.Utilities.Extensions;
using Fasterflect;
using Microsoft.Extensions.Logging;
using NetMQ;

namespace Eventually.Infrastructure.Transport.Messages
{
    public class WireMessageFactory : IWireMessageFactory
    {
        private readonly ILogger _outgoingLogger;
        private readonly ILogger _incomingLogger;

        public WireMessageFactory(ILoggerFactory loggerFactory)
        {
            _outgoingLogger = loggerFactory.CreateLogger($"{typeof(WireMessage).FullName}[Outgoing]");
            _incomingLogger = loggerFactory.CreateLogger($"{typeof(WireMessage).FullName}[Incoming]");
        }

        public IWireMessage<TMessage> Create<TMessage>(TMessage message, IUniquelyIdentified sender)
            where TMessage : IMessage
        {
            return Create(message, sender.Identity);
        }

        public IWireMessage<TMessage> Create<TMessage>(TMessage message, Guid senderId)
            where TMessage : IMessage
        {
            var wireMessage = new WireMessage<TMessage>(message, senderId);
            _outgoingLogger?.LogDebug($"Created a message of type `{wireMessage.MessageType.FullName}`: ```{wireMessage.Message.ToJson()}```");
            return wireMessage;
        }

        public IWireMessage Create(NetMQMessage message)
        {
            var parameters = WireMessage.GetParameters(message);
            var wireMessage = (IWireMessage) typeof(WireMessage<>).MakeGenericType(parameters.MessageType)
                .Constructor(typeof(Guid), typeof(byte[]))
                .Invoke(new object[] {parameters.SenderId, parameters.ContentBytes});

            _incomingLogger?.LogDebug($"Received a message from {wireMessage.SenderId} of type `{wireMessage.MessageType.FullName}`: ```{wireMessage.Message.ToJson()}```");
            return wireMessage;
        }
    }
}