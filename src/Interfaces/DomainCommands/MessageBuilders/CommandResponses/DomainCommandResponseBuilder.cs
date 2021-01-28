using System;
using System.Linq;
using Eventually.Utilities.Messages;

namespace Eventually.Interfaces.DomainCommands.MessageBuilders.CommandResponses
{
    public class DomainCommandResponseBuilder :
        MessageBuilder<DomainCommandResponse>,
        IDomainCommandResponseBuilderWithCauseSet,
        IDomainCommandResponseBuilderWithSuccessSet
    {
        private DomainCommandResponseBuilder() { }

        public static IDomainCommandResponseBuilderWithCauseSet InResponseTo(DomainCommand command)
        {
            return (IDomainCommandResponseBuilderWithCauseSet) new DomainCommandResponseBuilder()
                .With(response => response.CorrelationId, command.CorrelationId)
                .With(response => response.CommandId, command.Identity);
        }

        public IDomainCommandResponseBuilderWithSuccessSet Successful =>
            (IDomainCommandResponseBuilderWithSuccessSet) this
                .With(response => response.Succeeded, true);
        
        public IDomainCommandResponseBuilderWithSuccessSet Failed(Exception exception, params Exception[] exceptions)
        {
            foreach (var ex in new[]{exception}.Concat(exceptions))
            {
                this.With(response => response.Errors, DomainCommandResponseError.From(ex));
            }
            return this;
        }
    }
}