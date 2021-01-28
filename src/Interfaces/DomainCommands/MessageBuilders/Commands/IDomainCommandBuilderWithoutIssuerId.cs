using System;
using Eventually.Interfaces.Common.Messages;

namespace Eventually.Interfaces.DomainCommands.MessageBuilders.Commands
{
    public interface IDomainCommandBuilderWithoutIssuerId<TCommand, TNext> :
        IMessageBuilder<TCommand>
        where TCommand : DomainCommand
        where TNext : IMessageBuilder<TCommand>
    {
        TNext IssuedBy(Guid userId);
    }
}