using Eventually.Interfaces.DomainEvents;
using NEventStore.Domain;

namespace Eventually.Domain.EventBuilders
{
    public class EntityCreatedEventBuilder<TEvent> : DomainEventBuilder<TEvent>
        where TEvent : class, IEntityCreatedEvent
    {
        private EntityCreatedEventBuilder(IAggregate aggregate) : base(aggregate) { }

        public static IDomainEventBuilderWithoutCommandSet<TEvent> For<TAggregate>(TAggregate aggregate) where TAggregate : IAggregate
        {
            return new EntityCreatedEventBuilder<TEvent>(aggregate);
        }
    }
}