using System;

namespace Eventually.Interfaces.Common
{
    public interface IUniquelyIdentified
    {
        Guid Identity { get; }
    }
}