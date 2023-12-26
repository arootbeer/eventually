using Eventually.Interfaces.DomainCommands;

namespace Eventually.Portal.Domain.IAAA.Commands.Users
{
    public record AddLoginToUserCommand : ChangeEntityCommand
    {
        public string LoginProvider { get; init; }
        
        public string LoginHash { get; init; }
        
        public string ProviderDisplayName { get; init; }
    }
}