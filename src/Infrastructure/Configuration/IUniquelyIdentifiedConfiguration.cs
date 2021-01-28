using System;

namespace Eventually.Portal.Infrastructure.Configuration
{
    public interface IUniquelyIdentifiedConfiguration
    {
        Guid Identity { get; }
    }
}