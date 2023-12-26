namespace Eventually.Portal.Domain.Counter.Events
{
    public interface IGlobalCounterChangedEvent : IGlobalCounterEvent
    {
        decimal PreviousValue { get; }
    }
}