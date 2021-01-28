using Eventually.Interfaces.Common.Messages;

namespace Eventually.Interfaces.DomainCommands.MessageBuilders
{
    public interface IDomainEventBuilder<TEvent> :
        IMessageBuilder<TEvent>
        where TEvent : IMessage
    {
    }
}