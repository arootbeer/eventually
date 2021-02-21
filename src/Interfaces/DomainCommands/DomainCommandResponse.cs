using System;
using System.Collections.Generic;
using Eventually.Interfaces.Common.Messages;

namespace Eventually.Interfaces.DomainCommands
{
    public record DomainCommandResponse : Message
    {
        public Guid CommandId { get; init; }

        public bool Succeeded { get; init; }

        public IEnumerable<DomainCommandResponseError> Errors { get; init; }
    }
}