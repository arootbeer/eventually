using System;

namespace Eventually.Interfaces.DomainCommands.MessageBuilders.Commands
{
    public class ChangeEntityCommandBuilder<TCommand> :
        DomainCommandBuilder<TCommand, IChangeEntityCommandBuilder<TCommand>>,
        IChangeEntityCommandBuilder<TCommand>
        where TCommand : ChangeEntityCommand
    {
        private ChangeEntityCommandBuilder() { }
    }

    public class ChangeEntityCommandBuilder
    {
        public static ChangeEntityCommandBuilder<TCommand> For<TCommand>(Guid entityId) where TCommand : ChangeEntityCommand
        {
            var builder = (ChangeEntityCommandBuilder<TCommand>) Activator.CreateInstance(typeof(ChangeEntityCommandBuilder<TCommand>), true);
            return (ChangeEntityCommandBuilder<TCommand>) builder
                .With(cmd => cmd.EntityId, entityId);
        }
    }

}