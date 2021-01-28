using System;

namespace Eventually.Interfaces.DomainCommands
{
    public class ChangeEntityCommand : DomainCommand
    {
        public Guid EntityId { get; }
    }
}