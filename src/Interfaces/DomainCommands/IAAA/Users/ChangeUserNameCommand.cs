namespace Eventually.Interfaces.DomainCommands.IAAA.Users
{
    public record ChangeUserNameCommand : ChangeEntityCommand
    {
        public string Username { get; init; }
    }
}