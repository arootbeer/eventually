using System;

namespace Eventually.Interfaces.DomainCommands.MessageBuilders.Commands
{
    public class CreateEntityCommandBuilder<TCommand> :
        DomainCommandBuilder<TCommand, ICreateEntityCommandBuilder<TCommand>>,
        ICreateEntityCommandBuilder<TCommand>
        where TCommand : CreateEntityCommand
    {
        private CreateEntityCommandBuilder() { }

        public override ICreateEntityCommandBuilder<TCommand> IssuedBy(Guid userId)
        {
            return (ICreateEntityCommandBuilder<TCommand>) base.IssuedBy(userId)
                .With(command => command.CreatorId, userId);
        }
    }

    public class CreateEntityCommandBuilder
    {
        public static CreateEntityCommandBuilder<TCommand> For<TCommand>() where TCommand : CreateEntityCommand
        {
            return (CreateEntityCommandBuilder<TCommand>) Activator.CreateInstance(typeof(CreateEntityCommandBuilder<TCommand>), true);
        }
    }
}