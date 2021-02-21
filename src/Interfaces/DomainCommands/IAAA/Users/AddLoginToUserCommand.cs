namespace Eventually.Interfaces.DomainCommands.IAAA.Users
{
    public record AddLoginToUserCommand : ChangeEntityCommand
    {
        public string LoginProvider { get; init; }
        
        public string LoginHash { get; init; }
        
        public string ProviderDisplayName { get; init; }
    }
}