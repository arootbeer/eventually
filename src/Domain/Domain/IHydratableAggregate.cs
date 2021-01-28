using NEventStore.Domain;

namespace Eventually.Domain
{
    public interface IHydratableAggregate<in TMemento>
        where TMemento : IMemento
    {
        void RestoreFrom(TMemento memento);
    }
}