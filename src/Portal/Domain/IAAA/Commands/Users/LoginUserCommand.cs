using Eventually.Interfaces.DomainCommands;

namespace Eventually.Portal.Domain.IAAA.Commands.Users
{
    public record LoginUserCommand : ChangeEntityCommand
    {
        public string IPAddress { get; init; }

        public string Password { get; init; }

        // TODO: De-naive-ify
        // https://stackoverflow.com/questions/66015437
        public override string ToString()
        {
            return base.ToString()
                .Replace(Password, "********");
        }
    }
}