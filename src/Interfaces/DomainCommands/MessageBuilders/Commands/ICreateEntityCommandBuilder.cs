using Eventually.Interfaces.Common.Messages;

namespace Eventually.Interfaces.DomainCommands.MessageBuilders.Commands
{
    public interface ICreateEntityCommandBuilder<TCommand> :
        IMessageBuilder<TCommand>
        where TCommand : CreateEntityCommand
    {

    }
}