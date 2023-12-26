using Eventually.Interfaces.DomainEvents;

namespace Eventually.Portal.Domain.Counter.Events
{
    public class GlobalCounterCreated : DomainEntityCreatedEventBase, IGlobalCounterEvent
    {
    }
}