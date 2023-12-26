using Eventually.Interfaces.DomainCommands;

namespace Eventually.Portal.Domain.Counter.Commands
{
    public record DecrementGlobalCounterCommand : ChangeEntityCommand;
}