using System;
using Eventually.Interfaces.Common.Messages;

namespace Eventually.Infrastructure.Transport.Messages
{
    public interface IWireMessage<TMessage> : IWireMessage
        where TMessage : IMessage
    {
        new TMessage Message { get; }
    }

    public interface IWireMessage
    {
        Guid SenderId { get; }

        IMessage Message { get; }

        byte[] MessageBytes { get; }

        Type MessageType { get; }

        Guid CommandOrQueryId { get; }
    }
}