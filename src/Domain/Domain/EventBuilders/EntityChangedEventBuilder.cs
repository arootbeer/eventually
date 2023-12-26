using Eventually.Interfaces.DomainEvents;
using NEventStore.Domain;

namespace Eventually.Domain.EventBuilders
{
    public class EntityChangedEventBuilder<TEvent> : DomainEventBuilder<TEvent>
        where TEvent : class, IEntityChangedEvent
    {
        private EntityChangedEventBuilder(IAggregate aggregate) : base(aggregate) { }

        public static IDomainEventBuilderWithoutCommandSet<TEvent> For<TAggregate>(TAggregate aggregate) where TAggregate : IAggregate
        {
            return new EntityChangedEventBuilder<TEvent>(aggregate);
        }
    }
}