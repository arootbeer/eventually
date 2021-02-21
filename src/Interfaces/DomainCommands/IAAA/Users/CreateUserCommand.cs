namespace Eventually.Interfaces.DomainCommands.IAAA.Users
{
    public record CreateUserCommand : CreateEntityCommand
    {
        public string Username { get; init; }

        public string Password { get; init; }
    }
}