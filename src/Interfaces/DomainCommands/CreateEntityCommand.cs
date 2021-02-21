using System;

namespace Eventually.Interfaces.DomainCommands
{
    public record CreateEntityCommand : DomainCommand
    {
        public Guid CreatorId { get; init; }
    }
}