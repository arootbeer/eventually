using System;
using System.Linq;
using System.Threading;
using Eventually.Interfaces.Common;
using Eventually.Interfaces.DomainCommands;
using Eventually.Interfaces.DomainEvents;
using Fasterflect;
using NEventStore.Domain;
using NEventStore.Domain.Core;
using NodaTime;

namespace Eventually.Domain
{
    public abstract class AggregateBase<TAggregate, TCreateCommand, TEvent, TMemento> : AggregateBase<TAggregate, TCreateCommand, TEvent>, IHydratableAggregate<TMemento>
        where TAggregate : AggregateBase<TAggregate, TCreateCommand, TEvent>
        where TCreateCommand : CreateEntityCommand
        where TEvent : IDomainEvent
        where TMemento : IMemento
    {
        protected AggregateBase(Guid id) : base(id) { }

        protected AggregateBase(Guid id, IRouteEvents handler) : base(id, handler) { }

        public abstract void RestoreFrom(TMemento memento);
    }

    public abstract class AggregateBase<TAggregate, TCreateCommand, TEvent> : AggregateBase, IAggregate, IUniquelyIdentified
        where TAggregate : AggregateBase<TAggregate, TCreateCommand, TEvent>
        where TCreateCommand : CreateEntityCommand
        where TEvent : IDomainEvent
    {
        public static CancellationTokenSource TokenSource { private get; set; }

        protected readonly CancellationToken Token = TokenSource?.Token ?? default;

        protected AggregateBase(Guid id) : this(id, new Router()) { }

        protected AggregateBase(Guid id, IRouteEvents handler) : base(handler)
        {
            Id = id != Guid.Empty ? id : Guid.NewGuid();
        }

        public Guid Identity => Id;

        public Instant LastUpdated { get; private set; }

        void IAggregate.ApplyEvent(object domainEvent)
        {
            RegisteredRoutes.Dispatch(domainEvent);
            Version++;
        }

        public static TAggregate Create(TCreateCommand command)
        {
            var instance = (TAggregate)typeof(TAggregate).TryCreateInstanceWithValues(Guid.Empty);

            if (instance == null)
            {
                throw new Exception($"Could not find constructor `{typeof(TAggregate).Name}(Guid)` on type `{typeof(TAggregate).FullName}`.");
            }

            instance.ConsiderCreateCommand(command); //this can potentially throw

            instance.InitializeFromCreateCommand(command);
            return instance;
        }

        protected virtual void ConsiderCreateCommand(TCreateCommand command)
        {
            //default: allow creation with no validation
        }

        protected abstract void InitializeFromCreateCommand(TCreateCommand command);

        protected void Raise(TEvent domainEvent)
        {
            base.RaiseEvent(domainEvent);
        }

        private void Apply(TEvent domainEvent)
        {
            if (domainEvent is IEntityCreatedEvent)
            {
                Id = domainEvent.EntityId;
                LastUpdated = domainEvent.Timestamp;
            }
            else
            {
                if (Id != domainEvent.EntityId)
                {
                    throw new Exception($"Unable to replay event stream for {GetType().FullName}. An event of type {domainEvent.GetType().FullName} belonging to `{domainEvent.EntityId}` was encountered.");
                }

                // Domain events are supposed to be applied sequentially in version order. If this event is not either the next event (when hydrating) or
                // the current event (when dispatching after command execution), something has gone wrong.
                // There's an impedance mismatch here because NEventStore uses int for Version - the array type specification can be removed if that changes 
                if (!new long[] {Version, Version + 1}.Contains(domainEvent.EntityVersion))
                {
                    throw new Exception($"Unable to replay event stream for {GetType().FullName}. Events between version `{Version}` and `{domainEvent.EntityVersion}` were not found.");
                }
            }

            OnAllEvents(domainEvent);
        }

        /// <summary>
        /// Fired for every event.
        /// <p><b>Do not increment the entity version in overloads</b> - that is handled in
        /// the <see cref="IAggregate.ApplyEvent"/> implementation on <see cref="AggregateBase"/>
        /// </p>
        /// </summary>
        /// <param name="domainEvent"></param>
        protected virtual void OnAllEvents(TEvent domainEvent)
        {
        }

        [Obsolete("Use Raise(TEvent) instead", true)]
        protected new void RaiseEvent(object domainEvent)
        {
            if (!(domainEvent is TEvent typedEvent))
            {
                throw new Exception(
                    $"{GetType().FullName} only supports events of type {typeof(TEvent).Name}. " +
                    $"Event of type {domainEvent.GetType().FullName} provided cannot be handled."
                );
            }

            base.RaiseEvent(typedEvent);
        }

        private class Router : ConventionEventRouter
        {
            private TAggregate _aggregate;

            //since this router overrides the property update handling for the AggregateBase<TEvent> class,
            //allow AggregateBase<TEvent> subclasses to not define handlers
            public Router() : base(false) { }

            public override void Register(IAggregate aggregate)
            {
                base.Register(aggregate);

                _aggregate = (TAggregate) aggregate;
            }

            public override void Dispatch(object eventMessage)
            {
                _aggregate.Apply((TEvent) eventMessage);
                
                base.Dispatch(eventMessage);
            }
        }
    }
}