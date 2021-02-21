using System;

namespace Eventually.Interfaces.DomainCommands
{
    public record DomainCreateCommandResponse : DomainCommandResponse
    {
        public Guid CreatedEntityId { get; init; }
    }
}