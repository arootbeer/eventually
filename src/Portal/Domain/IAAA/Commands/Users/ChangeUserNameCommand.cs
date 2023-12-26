using Eventually.Interfaces.DomainCommands;

namespace Eventually.Portal.Domain.IAAA.Commands.Users
{
    public record ChangeUserNameCommand : ChangeEntityCommand
    {
        public string Username { get; init; }
    }
}