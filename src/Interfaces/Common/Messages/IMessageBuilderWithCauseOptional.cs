namespace Eventually.Interfaces.Common.Messages
{
    public interface IMessageBuilderWithCauseOptional<TMessage> :
        IMessageBuilder<TMessage>
        where TMessage : IMessage
    {
        public IMessageBuilder<TMessage> CausedBy(IMessage message);
    }
}