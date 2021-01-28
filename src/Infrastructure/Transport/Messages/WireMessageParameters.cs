using System;

namespace Eventually.Infrastructure.Transport.Messages
{
    public class WireMessageParameters
    {
        public WireMessageParameters(Type messageType, Guid senderId, byte[] contentBytes)
        {
            MessageType = messageType;
            SenderId = senderId;
            ContentBytes = contentBytes;
        }

        public Type MessageType { get; }

        public Guid SenderId { get; }

        public byte[] ContentBytes { get; }
    }
}