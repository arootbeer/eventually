using System;

namespace Eventually.Infrastructure.Configuration
{
    public interface IUniquelyIdentifiedConfiguration
    {
        Guid Identity { get; }
    }
}