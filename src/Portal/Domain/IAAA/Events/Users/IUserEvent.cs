using Eventually.Interfaces.DomainEvents;

namespace Eventually.Portal.Domain.IAAA.Events.Users
{
    public interface IUserEvent : IDomainEvent
    {
        public static string StreamId { get; } = "IAAA";
    }
}