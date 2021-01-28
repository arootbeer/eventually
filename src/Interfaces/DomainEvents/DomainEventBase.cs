using System;
using NodaTime;

namespace Eventually.Interfaces.DomainEvents
{
    public abstract class DomainEventBase : IDomainEvent
    {
        public Guid Identity { get; } = Guid.NewGuid();

        public Instant Timestamp { get; } = SystemClock.Instance.GetCurrentInstant();

        public Guid EntityId { get; }

        public int EntityVersion { get; }

        public Guid CorrelationId { get; }
    }
}