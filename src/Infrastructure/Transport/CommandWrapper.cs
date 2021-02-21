using System.Collections.Generic;

namespace Eventually.Infrastructure.Transport
{
    public record CommandWrapper
    {
        public string CommandType { get; init; }
        
        public IDictionary<string, object> CommandData { get; init; }
    }
}