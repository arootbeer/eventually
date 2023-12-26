using Eventually.Interfaces.DomainCommands;

namespace Eventually.Domain.Runtime
{
    public interface IDomainCommandExecutor
    {
        DomainCommandResponse Execute(DomainCommand command);
    }
}