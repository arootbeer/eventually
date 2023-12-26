using System;
using Eventually.Domain;
using Eventually.Domain.EventBuilders;
using Eventually.Portal.Domain.Counter.Commands;
using Eventually.Portal.Domain.Counter.Events;

namespace Eventually.Portal.Domain.Counter
{
    public class GlobalCounter : AggregateBase<GlobalCounter, CreateGlobalCounterCommand, IGlobalCounterEvent>
    {
        private decimal _value = decimal.Zero;
        
        private GlobalCounter() : base(Constants.GlobalCounterId) { }
        
        private GlobalCounter(Guid _) : base(Constants.GlobalCounterId) { }

        protected override void InitializeFromCreateCommand(CreateGlobalCounterCommand command)
        {
            Raise(
                EntityCreatedEventBuilder<GlobalCounterCreated>
                    .For(this)
                    .From(command)
                    .Build()
                );
        }

        public void Apply(GlobalCounterCreated cc) { }

        public void IncrementCounter(IncrementGlobalCounterCommand command)
        {
            Raise(
                EntityChangedEventBuilder<GlobalCounterIncremented>
                    .For(this)
                    .From(command)
                    .With(gci => gci.PreviousValue, _value)
                    .Build()
            );
        }

        public void Apply(GlobalCounterIncremented gci)
        {
            _value = gci.PreviousValue + 1;
        }

        public void RemoveUser(DecrementGlobalCounterCommand command)
        {
            Raise(
                EntityChangedEventBuilder<GlobalCounterDecremented>
                    .For(this)
                    .From(command)
                    .With(gcd => gcd.PreviousValue, _value)
                    .Build()
            );
        }

        public void Apply(GlobalCounterDecremented gcd)
        {
            _value = gcd.PreviousValue - 1;
        }
    }
}