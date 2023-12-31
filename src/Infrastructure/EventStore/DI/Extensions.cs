﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Eventually.Domain.EventHandling;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NEventStore;
using NEventStore.Domain;
using NEventStore.Domain.Core;
using NEventStore.Domain.Persistence;
using NEventStore.Persistence;

namespace Eventually.Infrastructure.EventStore.DI
{
    public static class Extensions
    {
        public static IHostBuilder UseEventStore(
            this IHostBuilder hostBuilder,
            IEnumerable<Assembly> eventHandlerAssemblies = null,
            Action<IServiceCollection> customConfiguration = null)
        {
            hostBuilder.ConfigureServices(
                (_, services) =>
                {
                    services.AddSingleton(EventStoreWireup.Factory);
                    services.AddSingleton(provider => provider.GetService<IStoreEvents>().Advanced);

                    services.AddTransient<IRepository, DebuggableRepository>();
                    services.AddTransient<IConstructAggregates, AggregateFactory>();
                    services.AddTransient<IDetectConflicts, ConflictDetector>();

                    var assemblies = eventHandlerAssemblies ?? AppDomain.CurrentDomain
                        .GetAssemblies();
                    
                    var handlerTypes = assemblies
                        .SelectMany(assembly => assembly.GetTypes())
                        .Where(type => typeof(IDomainEventHandler).IsAssignableFrom(type) && !type.IsAbstract && !type.IsInterface)
                        .ToList();
                    
                    handlerTypes.ForEach(type => AddSingletonMethod.MakeGenericMethod(typeof(IDomainEventHandler), type).Invoke(null, new object[] { services }));

                    customConfiguration?.Invoke(services);
                }
            );

            return hostBuilder;
        }

        private static readonly MethodInfo AddSingletonMethod = new Action<IServiceCollection>(AddSingleton<IDisposable, IDisposable>)
            .Method
            .GetGenericMethodDefinition();

        private static void AddSingleton<TService, TImplementation>(IServiceCollection services)
            where TService : class
            where TImplementation : class, TService
        {
            services.AddSingleton<TService, TImplementation>();
        }

        private sealed class DebuggableRepository : IRepository
        {
            private const string AggregateTypeHeader = "AggregateType";

            private readonly IDetectConflicts _conflictDetector;

            private readonly IStoreEvents _eventStore;

            private readonly IConstructAggregates _factory;

            private readonly IDictionary<string, ISnapshot> _snapshots = new Dictionary<string, ISnapshot>();

            private readonly IDictionary<string, IEventStream> _streams = new Dictionary<string, IEventStream>();

            public DebuggableRepository(IStoreEvents eventStore, IConstructAggregates factory, IDetectConflicts conflictDetector)
            {
                _eventStore = eventStore;
                _factory = factory;
                _conflictDetector = conflictDetector;
            }

            public void Dispose()
            {
                Dispose(true);
            }

            public TAggregate GetById<TAggregate>(Guid id) where TAggregate : class, IAggregate
            {
                return GetById<TAggregate>(Bucket.Default, id);
            }

            public TAggregate GetById<TAggregate>(Guid id, int versionToLoad) where TAggregate : class, IAggregate
            {
                return GetById<TAggregate>(Bucket.Default, id, versionToLoad);
            }

            public TAggregate GetById<TAggregate>(string bucketId, Guid id) where TAggregate : class, IAggregate
            {
                return GetById<TAggregate>(bucketId, id, int.MaxValue);
            }

            public TAggregate GetById<TAggregate>(string bucketId, Guid id, int versionToLoad) where TAggregate : class, IAggregate
            {
                var snapshot = GetSnapshot(bucketId, id, versionToLoad);
                var stream = OpenStream(bucketId, id, versionToLoad, snapshot);
                var aggregate = GetAggregate<TAggregate>(snapshot, stream);

                ApplyEventsToAggregate(versionToLoad, stream, aggregate);

                return aggregate as TAggregate;
            }

            public void Save(IAggregate aggregate, Guid commitId, Action<IDictionary<string, object>> updateHeaders)
            {
                Save(Bucket.Default, aggregate, commitId, updateHeaders);

            }

            public void Save(string bucketId, IAggregate aggregate, Guid commitId, Action<IDictionary<string, object>> updateHeaders)
            {
                var headers = PrepareHeaders(aggregate, updateHeaders);
                while (true)
                {
                    var stream = PrepareStream(bucketId, aggregate, headers);
                    var commitEventCount = stream.CommittedEvents.Count;

                    try
                    {
                        stream.CommitChanges(commitId);
                        aggregate.ClearUncommittedEvents();
                        return;
                    }
                    catch (DuplicateCommitException)
                    {
                        stream.ClearChanges();
                        // Issue: #4 and test: when_an_aggregate_is_persisted_using_the_same_commitId_twice
                        // should we rethtow the exception here? or provide a feedback whether the save was successful ?
                        return;
                    }
                    catch (ConcurrencyException e)
                    {
                        var conflict = ThrowOnConflict(stream, commitEventCount);
                        stream.ClearChanges();

                        if (conflict)
                        {
                            throw new ConflictingCommandException(e.Message, e);
                        }
                    }
                    catch (StorageException e)
                    {
                        throw new PersistenceException(e.Message, e);
                    }
                }
            }

            private void Dispose(bool disposing)
            {
                if (!disposing)
                {
                    return;
                }

                lock (_streams)
                {
                    foreach (var stream in _streams)
                    {
                        stream.Value.Dispose();
                    }

                    _snapshots.Clear();
                    _streams.Clear();
                }
            }

            private static void ApplyEventsToAggregate(int versionToLoad, IEventStream stream, IAggregate aggregate)
            {
                if (versionToLoad == 0 || aggregate.Version < versionToLoad)
                {
                    foreach (var @event in stream.CommittedEvents.Select(x => x.Body))
                    {
                        aggregate.ApplyEvent(@event);
                    }
                }
            }

            private IAggregate GetAggregate<TAggregate>(ISnapshot snapshot, IEventStream stream)
            {
                var memento = snapshot == null ? null : snapshot.Payload as IMemento;
                return _factory.Build(typeof(TAggregate), Guid.Parse(stream.StreamId), memento);
            }

            private ISnapshot GetSnapshot(string bucketId, Guid id, int version)
            {
                ISnapshot snapshot;
                var snapshotId = bucketId + id;
                if (!_snapshots.TryGetValue(snapshotId, out snapshot))
                {
                    _snapshots[snapshotId] = snapshot = _eventStore.Advanced.GetSnapshot(bucketId, id, version);
                }

                return snapshot;
            }

            private IEventStream OpenStream(string bucketId, Guid id, int version, ISnapshot snapshot)
            {
                IEventStream stream;
                var streamId = bucketId + "+" + id;
                if (_streams.TryGetValue(streamId, out stream))
                {
                    return stream;
                }

                stream = snapshot == null
                    ? _eventStore.OpenStream(bucketId, id, 0, version)
                    : _eventStore.OpenStream(snapshot, version);

                return _streams[streamId] = stream;
            }

            private IEventStream PrepareStream(string bucketId, IAggregate aggregate, Dictionary<string, object> headers)
            {
                IEventStream stream;
                var streamId = bucketId + "+" + aggregate.Id;
                if (!_streams.TryGetValue(streamId, out stream))
                {
                    _streams[streamId] = stream = _eventStore.CreateStream(bucketId, aggregate.Id);
                }

                foreach (var item in headers)
                {
                    stream.UncommittedHeaders[item.Key] = item.Value;
                }

                aggregate.GetUncommittedEvents()
                    .Cast<object>()
                    .Select(x => new EventMessage { Body = x })
                    .ToList()
                    .ForEach(stream.Add);

                return stream;
            }

            private static Dictionary<string, object> PrepareHeaders(
                IAggregate aggregate, Action<IDictionary<string, object>> updateHeaders)
            {
                var headers = new Dictionary<string, object>();

                headers[AggregateTypeHeader] = aggregate.GetType().FullName;
                if (updateHeaders != null)
                {
                    updateHeaders(headers);
                }

                return headers;
            }

            private bool ThrowOnConflict(IEventStream stream, int skip)
            {
                var committed = stream.CommittedEvents.Skip(skip).Select(x => x.Body);
                var uncommitted = stream.UncommittedEvents.Select(x => x.Body);
                return _conflictDetector.ConflictsWith(uncommitted, committed);
            }
        }
    }
}