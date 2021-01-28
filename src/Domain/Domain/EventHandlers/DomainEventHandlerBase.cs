using Eventually.Interfaces.Common;
using Eventually.Interfaces.DomainEvents;
using Microsoft.Extensions.Logging;

namespace Eventually.Domain.EventHandlers
{
    public abstract class DomainEventHandlerBase<TEvent> : IDomainEventHandler<TEvent>, IOptionallySequenced
        where TEvent : class, IDomainEvent
    {
        public int? Sequence => null;

        protected readonly ILogger Logger;

        protected DomainEventHandlerBase(ILoggerFactory loggerFactory)
        {
            Logger = loggerFactory.CreateLogger(GetType());
        }

        protected bool HandlesCovariantEvents { get; set; }

        public bool CanHandle(object domainEvent)
        {
            var tEvent = domainEvent as TEvent;
            if (HandlesCovariantEvents && tEvent != null)
            {
                return CanHandleInternal(tEvent);
            }

            return domainEvent.GetType() == typeof(TEvent) && CanHandleInternal(tEvent);
        }

        protected virtual bool CanHandleInternal(TEvent domainEvent)
        {
            return true;
        }

        public void Handle(TEvent domainEvent)
        {
            HandleInternal(domainEvent);
        }

        protected abstract void HandleInternal(TEvent domainEvent);
    }
}