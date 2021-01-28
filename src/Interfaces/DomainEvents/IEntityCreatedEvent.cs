using System;

namespace Eventually.Interfaces.DomainEvents
{
    public interface IEntityCreatedEvent : IDomainEvent
    {
        Guid CreatorId { get; }
    }
}