using System;

namespace Eventually.Interfaces.Common
{
    public interface IUniquelyIdentified
    {
        Guid Identity { get; }
    }

    public interface IOptionallySequenced
    {
        int? Sequence { get; }
    }
}