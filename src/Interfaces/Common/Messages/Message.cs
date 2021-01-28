using System;
using NodaTime;

namespace Eventually.Interfaces.Common.Messages
{
    public class Message : IMessage
    {
        public Guid Identity { get; }

        public Guid CorrelationId { get; }

        public Instant Timestamp { get; }
    }
}