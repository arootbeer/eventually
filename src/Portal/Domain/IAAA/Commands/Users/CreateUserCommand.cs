using Eventually.Interfaces.DomainCommands;

namespace Eventually.Portal.Domain.IAAA.Commands.Users
{
    public record CreateUserCommand : CreateEntityCommand
    {
        public string Username { get; init; }

        public string Password { get; init; }
    }
}