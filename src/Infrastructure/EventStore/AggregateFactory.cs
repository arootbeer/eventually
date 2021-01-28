using System;
using System.Linq;
using Eventually.Domain;
using Fasterflect;
using NEventStore.Domain;
using NEventStore.Domain.Persistence;

namespace Eventually.Infrastructure.EventStore
{
    public class AggregateFactory : IConstructAggregates
    {
        public IAggregate Build(Type type, Guid id, IMemento snapshot)
        {
            var instance = (IAggregate) type.TryCreateInstanceWithValues(id);

            if (instance == null)
            {
                throw new Exception($"Could not find constructor `{type.Name}(Guid)` on type `{type.FullName}`.");
            }

            if (snapshot != null)
            {
                var hydratableType = typeof(IHydratableAggregate<>).MakeGenericType(snapshot.GetType());
                if (type.GetInterfaces().Any(iface => iface == hydratableType))
                {
                    instance.CallMethod(nameof(IHydratableAggregate<IMemento>.RestoreFrom), snapshot);
                }
            }
            return instance;
        }
    }
}