using Eventually.Interfaces.DomainEvents;

namespace Eventually.Portal.Domain.Counter.Events
{
    public class GlobalCounterIncremented : DomainEntityChangedEventBase, IGlobalCounterChangedEvent
    {
        public decimal PreviousValue { get; init; }
    }
}