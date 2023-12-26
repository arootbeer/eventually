using System.Text;
using Eventually.Interfaces.DomainCommands;

namespace Eventually.Portal.Domain.IAAA.Commands.Users
{
    public record LoginUserCommand : ChangeEntityCommand
    {
        public string IPAddress { get; init; }

        public string Password { get; init; }

        public override string ToString()
        {
            var builder = new StringBuilder();
            (this with { Password = null }).PrintMembers(builder);
            return builder.ToString();
        }
    }
}