using System;
using System.Collections.Generic;
using Eventually.Infrastructure.Transport;
using Eventually.Interfaces.Common;
using Eventually.Interfaces.DomainCommands;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;

namespace Eventually.Domain.APIHost.ModelBinding
{
    public class CommandModelBinderProvider : IModelBinderProvider
    {
        private readonly IList<IInputFormatter> _formatters;
        private readonly ILoggerFactory _loggerFactory;
        private readonly IEnumerable<MessageTypeLookupStrategy> _messageTypeLookupStrategies;
        private readonly MvcOptions _options;
        private readonly IHttpRequestStreamReaderFactory _readerFactory;

        private CommandModelBinder _modelBinder;

        public CommandModelBinderProvider(
            MvcOptions options,
            IHttpRequestStreamReaderFactory readerFactory,
            ILoggerFactory loggerFactory,
            IEnumerable<MessageTypeLookupStrategy> messageTypeLookupStrategies
        )
        {
            _options = options;
            _formatters = options.InputFormatters;
            _readerFactory = readerFactory;
            _loggerFactory = loggerFactory;
            _messageTypeLookupStrategies = messageTypeLookupStrategies;
        }
        public IModelBinder GetBinder(ModelBinderProviderContext context)
        {
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (context.BindingInfo.BindingSource != null &&
                context.BindingInfo.BindingSource.CanAcceptDataFrom(BindingSource.Body) &&
                context.Metadata.ModelType == typeof(DomainCommand))
            {
                if (_modelBinder != null)
                {
                    return _modelBinder;
                }
                
                if (_formatters.Count == 0)
                {
                    throw new InvalidOperationException("No formatters found");
                }

                var dictionaryMetadata = context.MetadataProvider.GetMetadataForType(typeof(CommandWrapper));
                
                _modelBinder = new CommandModelBinder(
                    _formatters,
                    _readerFactory,
                    _messageTypeLookupStrategies,
                    _options,
                    dictionaryMetadata,
                    _loggerFactory
                );
            }

            return _modelBinder;
        }
    }
}