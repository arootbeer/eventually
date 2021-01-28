using System;

namespace Eventually.Interfaces.DomainEvents
{
    public abstract class CreatedEventBase : DomainEventBase, IEntityCreatedEvent
    {
        public Guid CreatorId { get; }
    }
}