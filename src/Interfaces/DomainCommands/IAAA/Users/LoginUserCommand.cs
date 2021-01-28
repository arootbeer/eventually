namespace Eventually.Interfaces.DomainCommands.IAAA.Users
{
    public class LoginUserCommand : ChangeEntityCommand
    {
        public string IPAddress { get; }

        public string Password { get; }
    }
}