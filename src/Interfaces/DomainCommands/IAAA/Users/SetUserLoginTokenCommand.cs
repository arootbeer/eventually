namespace Eventually.Interfaces.DomainCommands.IAAA.Users
{
    public record SetUserLoginTokenCommand : ChangeEntityCommand
    {
        public string LoginProvider { get; init; }
        
        public string TokenName { get; init; }
        
        public string TokenValue { get; init; }
    }
}