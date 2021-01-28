namespace Eventually.Interfaces.DomainCommands.IAAA.Users
{
    public class CreateUserCommand : CreateEntityCommand
    {
        public string Username { get; }

        public string Password { get; }
    }
}