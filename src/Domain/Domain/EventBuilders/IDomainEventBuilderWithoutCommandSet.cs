using Eventually.Interfaces.DomainCommands;
using Eventually.Interfaces.DomainCommands.MessageBuilders;
using Eventually.Interfaces.DomainEvents;

namespace Eventually.Domain.EventBuilders
{
    public interface IDomainEventBuilderWithoutCommandSet<TEvent> 
        where TEvent : class, IDomainEvent
    {
        IDomainEventBuilder<TEvent> From<TCommand>(TCommand command) where TCommand : DomainCommand;
    }
}