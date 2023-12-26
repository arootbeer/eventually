using System;
using System.Collections.Generic;

namespace Eventually.Domain.Runtime
{
    public interface IKnownAggregateTypeProvider
    {
        IReadOnlyCollection<Type> GetTypes();
    }
}