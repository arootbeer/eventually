using System;
using System.Linq;
using Eventually.Utilities.Messages;
using NEventStore.Domain;

namespace Eventually.Interfaces.DomainCommands.MessageBuilders.CommandResponses
{
    public class DomainCreateCommandResponseBuilder : 
        MessageBuilder<DomainCreateCommandResponse>,
        IDomainCreateCommandResponseBuilderWithCauseSet,
        IDomainCreateCommandResponseBuilderWithSuccessSet
    {
        protected DomainCreateCommandResponseBuilder() { }

        public static IDomainCreateCommandResponseBuilderWithCauseSet InResponseTo(CreateEntityCommand command)
        {
            return (IDomainCreateCommandResponseBuilderWithCauseSet) new DomainCreateCommandResponseBuilder()
                .With(response => response.CorrelationId, command.CorrelationId)
                .With(response => response.CommandId, command.Identity);

        }

        IDomainCreateCommandResponseBuilderWithSuccessSet IDomainCreateCommandResponseBuilderWithCauseSet.ResultingInCreationOf(IAggregate entity)
        {
            return (IDomainCreateCommandResponseBuilderWithSuccessSet) this
                .With(response => response.CreatedEntityId, entity.Id)
                .With(response => response.Succeeded, true);
        }

        public IDomainCreateCommandResponseBuilderWithSuccessSet Failed(Exception exception, params Exception[] exceptions)
        {
            foreach (var ex in new[] { exception }.Concat(exceptions))
            {
                this.With(response => response.Errors, DomainCommandResponseError.From(ex));
            }
            return (IDomainCreateCommandResponseBuilderWithSuccessSet) this
                .With(response => response.Succeeded, true);
        }
    }
}