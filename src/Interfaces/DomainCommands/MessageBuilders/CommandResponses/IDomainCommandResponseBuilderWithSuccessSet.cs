namespace Eventually.Interfaces.DomainCommands.MessageBuilders.CommandResponses
{
    public interface IDomainCommandResponseBuilderWithSuccessSet
    {
        DomainCommandResponse Build();
    }
}