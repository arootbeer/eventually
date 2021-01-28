using Eventually.Interfaces.DomainCommands;
using Eventually.Interfaces.DomainCommands.MessageBuilders;
using Eventually.Interfaces.DomainEvents;
using Eventually.Utilities.Messages;
using NEventStore.Domain;

namespace Eventually.Domain.EventBuilders
{
    public class DomainEventBuilder<TEvent> :
        MessageBuilder<TEvent>,
        IDomainEventBuilder<TEvent>,
        IDomainEventBuilderWithoutCommandSet<TEvent>
        where TEvent : class, IDomainEvent
    {
        public DomainEventBuilder(IAggregate aggregate)
        {
            this
                .With(domainEvent => domainEvent.EntityId, aggregate.Id)
                .With(domainEvent => domainEvent.EntityVersion, aggregate.Version);
        }

        public IDomainEventBuilder<TEvent> From<TCommand>(TCommand command) where TCommand : DomainCommand
        {
            if (typeof(IEntityCreatedEvent).IsAssignableFrom(typeof(TEvent)) && command is CreateEntityCommand createCommand)
            {
                this
                    .With(domainEvent => ((IEntityCreatedEvent) domainEvent).CreatorId, createCommand.CreatorId);
            }

            return (IDomainEventBuilder<TEvent>) this
                .With(domainEvent => domainEvent.CorrelationId, command.CorrelationId);
        }
    }
}