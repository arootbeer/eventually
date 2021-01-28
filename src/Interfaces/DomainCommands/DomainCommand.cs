using System;
using Eventually.Interfaces.Common.Messages;

namespace Eventually.Interfaces.DomainCommands
{
    public class DomainCommand : Message
    {
        public Guid IssuingUserId { get; }
    }
}