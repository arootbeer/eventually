using System;
using Eventually.Interfaces.Common.Messages;

namespace Eventually.Interfaces.DomainEvents
{
    public interface IDomainEvent : IMessage
    {
        public Guid EntityId { get; }

        public int EntityVersion { get; }
    }
}