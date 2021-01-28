using System;
using NEventStore.Domain;

namespace Eventually.Interfaces.DomainCommands.MessageBuilders.CommandResponses
{
    public interface IDomainCreateCommandResponseBuilderWithCauseSet
    {
        IDomainCreateCommandResponseBuilderWithSuccessSet ResultingInCreationOf(IAggregate entity);

        IDomainCreateCommandResponseBuilderWithSuccessSet Failed(Exception exception, params Exception[] exceptions);
    }
}