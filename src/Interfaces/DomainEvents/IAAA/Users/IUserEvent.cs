namespace Eventually.Interfaces.DomainEvents.IAAA.Users
{
    public interface IUserEvent : IDomainEvent
    {
        public static string StreamId { get; } = "IAAA";
    }
}