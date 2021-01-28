using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Eventually.Domain.EventHandlers;
using Eventually.Infrastructure.EventStore.Configuration;
using Eventually.Interfaces.DomainEvents;
using Eventually.Utilities.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NEventStore;
using NEventStore.Persistence;

namespace Eventually.Infrastructure.EventStore
{
    public class EventStoreListenerService : BackgroundService
    {
        private readonly IPersistStreams _streams;
        private readonly IEventStoreConfiguration _configuration;
        private readonly ILastCommitCheckpointProvider _checkpointProvider;
        private readonly IDictionary<Type, ConcurrentBag<IDomainEventHandler>> _eventHandlers = new ConcurrentDictionary<Type, ConcurrentBag<IDomainEventHandler>>();
        private readonly IDictionary<Type, MethodInfo> _handlerMethods = new ConcurrentDictionary<Type, MethodInfo>();
        private readonly ILogger<EventStoreListenerService> _logger;

        private readonly PollingClient _pollingClient;

        public EventStoreListenerService(
            IPersistStreams streams,
            IEventStoreConfiguration configuration,
            IEnumerable<IDomainEventHandler> eventHandlers,
            ILastCommitCheckpointProvider checkpointProvider,
            ILogger<EventStoreListenerService> logger
        )
        {
            _streams = streams;
            _configuration = configuration;
            _checkpointProvider = checkpointProvider;
            _logger = logger;

            foreach (var eventHandler in eventHandlers)
            {
                var eventType = eventHandler.EventType;
                if (!_eventHandlers.ContainsKey(eventType))
                {
                    _eventHandlers.Add(eventType, new ConcurrentBag<IDomainEventHandler>());
                }

                _eventHandlers[eventType].Add(eventHandler);
            }

            _pollingClient = new PollingClient(_streams, HandleCommit, _logger);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.Register(() => _pollingClient.Stop());

            await _pollingClient.StartFrom(_checkpointProvider.GetLastCheckpointToken);
        }

        private int _numberOfFailures = 0;
        private PollingClient.HandlingResult HandleCommit(ICommit commit)
        {
            var result = PollingClient.HandlingResult.MoveToNext;
            foreach (var domainEvent in commit.Events.Select(e => e.Body).Cast<IDomainEvent>())
            {
                var eventType = domainEvent.GetType();
                if (!_eventHandlers.ContainsKey(eventType))
                {
                    continue;
                }

                if (!_handlerMethods.ContainsKey(eventType))
                {
                    _handlerMethods.Add(eventType, InvokeHandlerMethod.MakeGenericMethod(eventType));
                }

                foreach (var handler in _eventHandlers[eventType].Where(handler => handler.CanHandle(domainEvent)))
                {
                    var handlerType = handler.GetType();
                    
                    try
                    {
                        _logger.LogInformation($"Invoking `{handlerType.Name}` for `{eventType.Name}`{Environment.NewLine}```{domainEvent.ToJson()}```");
                        _handlerMethods[eventType].Invoke(null, new object[] {handler, domainEvent});
                        Interlocked.Exchange(ref _numberOfFailures, 0);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Exception encountered while attempting to handle `{eventType.Name}`" +
                                             $" with version `{domainEvent.EntityVersion}` for `{domainEvent.EntityId}`" +
                                             $" using `{handlerType.FullName}`. Requesting retry.");
                        Thread.Sleep(10000);
                        if (_numberOfFailures > 5)
                        {
                            throw;
                        }

                        Interlocked.Increment(ref _numberOfFailures);
                        result = PollingClient.HandlingResult.Retry;
                    }
                }
            }

            _checkpointProvider.SetLastCheckpointToken(commit.BucketId, commit.CheckpointToken);
            return result;
        }

        private static readonly MethodInfo InvokeHandlerMethod = 
            new Action<IDomainEventHandler<IDomainEvent>, IDomainEvent>(InvokeHandler)
                .Method
                .GetGenericMethodDefinition();

        private static void InvokeHandler<TEvent>(IDomainEventHandler<TEvent> handler, TEvent domainEvent)
            where TEvent : class, IDomainEvent
        {
            handler.Handle(domainEvent);
        }

        public override void Dispose()
        {
            _pollingClient.Dispose();
        }
    }
}