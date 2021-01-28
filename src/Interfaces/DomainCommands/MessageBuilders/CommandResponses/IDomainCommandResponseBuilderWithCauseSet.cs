using System;

namespace Eventually.Interfaces.DomainCommands.MessageBuilders.CommandResponses
{
    public interface IDomainCommandResponseBuilderWithCauseSet
    {
        IDomainCommandResponseBuilderWithSuccessSet Successful { get; }

        IDomainCommandResponseBuilderWithSuccessSet Failed(Exception exception, params Exception[] exceptions);
    }
}