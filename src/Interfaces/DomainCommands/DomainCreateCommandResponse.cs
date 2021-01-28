using System;

namespace Eventually.Interfaces.DomainCommands
{
    public class DomainCreateCommandResponse : DomainCommandResponse
    {
        public Guid CreatedEntityId { get; }
    }
}