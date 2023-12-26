using Eventually.Interfaces.DomainCommands;

namespace Eventually.Portal.Domain.IAAA.Commands.Users
{
    public record SetUserLoginTokenCommand : ChangeEntityCommand
    {
        public string LoginProvider { get; init; }
        
        public string TokenName { get; init; }
        
        public string TokenValue { get; init; }
    }
}