using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Eventually.Interfaces.Common.Messages
{
    public interface IMessageBuilder<TMessage>
        where TMessage : IMessage
    {
        IMessageBuilder<TMessage> With<T>(Expression<Func<TMessage, T>> property, T value);

        IMessageBuilder<TMessage> With<T>(Expression<Func<TMessage, IEnumerable<T>>> property, T value);

        TMessage Build();
    }
}