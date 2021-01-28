using System;
using System.Collections.Generic;
using Eventually.Interfaces.Common.Messages;

namespace Eventually.Interfaces.DomainCommands
{
    public class DomainCommandResponse : Message
    {
        public Guid CommandId { get; }

        public bool Succeeded { get; }

        public IEnumerable<DomainCommandResponseError> Errors { get; }
    }
}