using System;
using Eventually.Interfaces.Common;
using Eventually.Interfaces.Common.Messages;
using NetMQ;

namespace Eventually.Infrastructure.Transport.Messages
{
    public interface IWireMessageFactory
    {
        IWireMessage<TMessage> Create<TMessage>(TMessage message, IUniquelyIdentified sender) where TMessage : IMessage;

        IWireMessage<TMessage> Create<TMessage>(TMessage message, Guid senderId) where TMessage : IMessage;
        
        IWireMessage Create(NetMQMessage message);
    }
}