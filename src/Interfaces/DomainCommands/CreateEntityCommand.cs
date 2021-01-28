using System;

namespace Eventually.Interfaces.DomainCommands
{
    public class CreateEntityCommand : DomainCommand
    {
        public Guid CreatorId { get; }
    }
}