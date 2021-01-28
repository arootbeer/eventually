namespace Eventually.Interfaces.DomainCommands.IAAA.Users
{
    public class ChangeUserNameCommand : ChangeEntityCommand
    {
        public string Username { get; }
    }
}