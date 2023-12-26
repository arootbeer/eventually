using System;

namespace Eventually.Interfaces.DomainEvents
{
    public abstract class DomainEntityCreatedEventBase : DomainEventBase, IEntityCreatedEvent
    {
        public Guid CreatorId { get; }
    }
}