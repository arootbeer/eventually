using System;
using Eventually.Interfaces.DomainEvents;

namespace Eventually.Domain.EventHandling
{
    public interface IDomainEventHandler
    {
        Type EventType { get; }

        bool CanHandle(object domainEvent);
    }

    public interface IDomainEventHandler<in TEvent> : IDomainEventHandler
        where TEvent : class, IDomainEvent
    {
        Type IDomainEventHandler.EventType => typeof(TEvent);

        void Handle(TEvent domainEvent);
    }
}