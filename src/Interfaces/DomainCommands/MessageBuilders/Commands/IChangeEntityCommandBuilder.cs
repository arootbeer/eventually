using Eventually.Interfaces.Common.Messages;

namespace Eventually.Interfaces.DomainCommands.MessageBuilders.Commands
{
    public interface IChangeEntityCommandBuilder<TCommand> :
        IMessageBuilder<TCommand>
        where TCommand : ChangeEntityCommand
    {
    }
}