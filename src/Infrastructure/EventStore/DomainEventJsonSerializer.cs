using System.Collections.Generic;
using Eventually.Utilities.Serialization;
using NEventStore;
using NEventStore.Serialization;

namespace Eventually.Infrastructure.EventStore
{
    public class DomainEventJsonSerializer : ImmutableTypesJsonSerializer, ISerialize
    {
        public DomainEventJsonSerializer() : base(typeof(List<EventMessage>)) { }
    }
}