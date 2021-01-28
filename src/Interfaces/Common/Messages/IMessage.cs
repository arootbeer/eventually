using System;
using NodaTime;

namespace Eventually.Interfaces.Common.Messages
{
    public interface IMessage : IUniquelyIdentified
    {
        Guid CorrelationId { get; }

        Instant Timestamp { get; }
    }
}