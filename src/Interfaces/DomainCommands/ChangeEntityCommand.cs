using System;

namespace Eventually.Interfaces.DomainCommands
{
    public record ChangeEntityCommand : DomainCommand
    {
        public Guid EntityId { get; init; }
    }
}