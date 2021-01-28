using System;
using Eventually.Interfaces.Common.Messages;
using Eventually.Utilities.Messages;

namespace Eventually.Interfaces.DomainCommands.MessageBuilders.Commands
{
    public abstract class DomainCommandBuilder<TCommand, TNext> :
        MessageBuilder<TCommand>,
        IDomainCommandBuilderWithoutIssuerId<TCommand, TNext>
        where TCommand : DomainCommand
        where TNext : class, IMessageBuilder<TCommand>
    {
        public virtual TNext IssuedBy(Guid userId)
        {
            return (TNext) this.With(command => command.IssuingUserId, userId);
        }
    }
}