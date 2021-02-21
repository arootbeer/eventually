using System;
using NodaTime;

namespace Eventually.Interfaces.Common.Messages
{
    public record Message : IMessage
    {
        public Guid Identity { get; init; }

        public Guid CorrelationId { get; init; }

        public Instant Timestamp { get; init; }
    }
}