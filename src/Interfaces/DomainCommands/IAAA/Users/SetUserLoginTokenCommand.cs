namespace Eventually.Interfaces.DomainCommands.IAAA.Users
{
    public class SetUserLoginTokenCommand : ChangeEntityCommand
    {
        public string LoginProvider { get; }
        
        public string TokenName { get; }
        
        public string TokenValue { get; }
    }
}