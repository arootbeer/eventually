namespace Eventually.Interfaces.DomainCommands.IAAA.Users
{
    public class AddLoginToUserCommand : ChangeEntityCommand
    {
        public string LoginProvider { get; }
        
        public string LoginHash { get; }
        
        public string ProviderDisplayName { get; }
    }
}