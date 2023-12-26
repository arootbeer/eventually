using System;
using System.Collections.Generic;
using System.Linq;
using Eventually.Utilities.Extensions;
using Microsoft.Extensions.Logging;

namespace Eventually.Domain.Runtime
{
    public class KnownAggregateTypeProvider : IKnownAggregateTypeProvider
    {
        private readonly ILogger _logger;

        public KnownAggregateTypeProvider(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger(nameof(KnownAggregateTypeProvider));
        }
        
        public IReadOnlyCollection<Type> GetTypes()
        {
            // TODO: replace this with something like MEF? 
            var aggregateTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(type => type.IsAssignableToGenericType(typeof(AggregateBase<,,>)))
                .Where(type => !type.IsAbstract).ToList();
            
            _logger.LogDebug(
                "Loaded {knownTypeCount} aggregate types: {knownTypes}",
                aggregateTypes.Count,
                string.Join(Environment.NewLine, aggregateTypes.Select(t => t.FullName))
            );
            
            return aggregateTypes;
        }
    }
}