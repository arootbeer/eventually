using System;
using Eventually.Interfaces.Common.Messages;

namespace Eventually.Interfaces.DomainCommands
{
    public record DomainCommand : Message
    {
        public Guid IssuingUserId { get; init; }
    }
}