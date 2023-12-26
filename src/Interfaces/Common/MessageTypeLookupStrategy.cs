using System;
using System.Collections.Immutable;

namespace Eventually.Interfaces.Common
{
    public abstract class MessageTypeLookupStrategy
    {
        protected MessageTypeLookupStrategy()
        {
            KnownTypes = GetType().Assembly
                .GetTypes()
                .ToImmutableDictionary(t => t.FullName, t => t);
        }
        
        protected ImmutableDictionary<string, Type> KnownTypes { get; }

        public virtual bool HasMessageType(string typeFullName) => KnownTypes.ContainsKey(typeFullName);

        public virtual Type GetMessageType(string typeFullName) => KnownTypes[typeFullName];
    }
}