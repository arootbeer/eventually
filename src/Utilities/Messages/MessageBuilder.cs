using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Eventually.Interfaces.Common.Messages;
using Eventually.Utilities.Reflection;
using NodaTime;

namespace Eventually.Utilities.Messages
{
    public abstract class MessageBuilder<TMessage> : 
        IMessageBuilder<TMessage>
        where TMessage : class, IMessage
    {
        private readonly Dictionary<string, object> _valueProvider = new();

        public IMessageBuilder<TMessage> With<T>(Expression<Func<TMessage, T>> property, T value)
        {
            var expression = (MemberExpression)property.Body;
            _valueProvider.Add(expression.Member.Name, value);

            return this;
        }

        public IMessageBuilder<TMessage> With<T>(Expression<Func<TMessage, IEnumerable<T>>> property, T value)
        {
            var expression = (MemberExpression)property.Body;
            var propertyName = expression.Member.Name;

            _valueProvider.TryGetValue(propertyName, out var existingValue);
            if (existingValue == null)
            {
                _valueProvider.Add(propertyName, new List<T>());
            }

            existingValue = _valueProvider[propertyName];
            if (!(existingValue is ICollection<T> collection))
            {
                throw new Exception($"Found a value of type `{existingValue.GetType().FullName}` for `{typeof(TMessage).Name}.{propertyName}`, but expected `ICollection<T>`");
            }
            collection.Add(value);

            return this;
        }

        public virtual TMessage Build()
        {
            if (!_valueProvider.ContainsKey(nameof(IMessage.Timestamp)))
            {
                _valueProvider.Add(nameof(IMessage.Timestamp), SystemClock.Instance.GetCurrentInstant());
            }
            if (!_valueProvider.ContainsKey(nameof(IMessage.Identity)))
            {
                _valueProvider.Add(nameof(IMessage.Identity), Guid.NewGuid());
            }
            if (!_valueProvider.ContainsKey(nameof(IMessage.CorrelationId)))
            {
                _valueProvider.Add(nameof(IMessage.CorrelationId), Guid.NewGuid());
            }

            return CustomActivator.CreateInstanceAndPopulateProperties<TMessage>(_valueProvider);
        }
    }
}