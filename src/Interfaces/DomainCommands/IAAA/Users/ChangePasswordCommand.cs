namespace Eventually.Interfaces.DomainCommands.IAAA.Users
{
    public class ChangePasswordCommand : ChangeEntityCommand
    {
        public string Password { get; }
    }
}